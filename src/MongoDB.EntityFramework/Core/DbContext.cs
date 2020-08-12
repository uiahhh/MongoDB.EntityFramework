using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.EntityFramework.Conventions;
using MongoDB.EntityFramework.Serializers;

namespace MongoDB.EntityFramework.Core
{
    public class DbContext : IDbContext
    {
        private readonly IMongoDatabase database;

        private readonly IDbContextOptions options;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        //id,entity
        private ConcurrentDictionary<string, ConcurrentDictionary<object, object>> collectionsFromContext;

        //id,entity serialized
        private ConcurrentDictionary<string, ConcurrentDictionary<object, OriginalValue>> collectionsOriginal;

        //id,state
        private ConcurrentDictionary<string, ConcurrentDictionary<object, EntityState>> collectionsState;

        private static bool globalConfigurationsInitialized = false;

        private static readonly object locker = new object();

        public DbContext(IMongoClient client, string databaseName, IDbContextOptions options = null)
        {
            this.database = client.GetDatabase(databaseName);
            this.options = options;
            this.InitCollectionsContext();
            this.InitOptions();
        }

        private void InitOptions()
        {
            if (this.options != null && this.options.AnyOptionEnabled && !globalConfigurationsInitialized)
            {
                lock (locker)
                {
                    if (!globalConfigurationsInitialized)
                    {
                        if (this.options.EnableStructSerializer)
                        {
                            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());
                        }

                        if (this.options.EnableMappingReadOnlyProperties)
                        {
                            ConventionRegistry.Register(nameof(MappingReadOnlyPropertiesConvention), new ConventionPack { new MappingReadOnlyPropertiesConvention() }, _ => true);
                        }

                        globalConfigurationsInitialized = true;
                    }
                }
            }
        }

        private void InitCollectionsContext()
        {
            this.collectionsFromContext = new ConcurrentDictionary<string, ConcurrentDictionary<object, object>>();
            this.collectionsOriginal = new ConcurrentDictionary<string, ConcurrentDictionary<object, OriginalValue>>();
            this.collectionsState = new ConcurrentDictionary<string, ConcurrentDictionary<object, EntityState>>();
        }

        private string GetIdFieldName<TEntity>()
        {
            //TODO: id field/column name
            return "_id";
        }

        private string GetIdFieldName(string collectionName)
        {
            //TODO: id field/column name
            return "_id";
        }

        protected string GetCollectionName<TEntity>()
            where TEntity : class
        {
            // TODO: necessidade de ter algo configuravel, como no fluent do EF
            return typeof(TEntity).Name;
        }

        protected IMongoCollection<TEntity> GetCollection<TEntity>()
            where TEntity : class
        {
            var collectionName = this.GetCollectionName<TEntity>();

            return this.database.GetCollection<TEntity>(collectionName);
        }

        public IDbSet<TEntity, TId> Set<TEntity, TId>()
            where TEntity : class
        {
            //TODO: fazer cache com um dicionario Type,Object
            return new DbSet<TEntity, TId>(this);
        }

        public async Task<List<TEntity>> AllAsync<TEntity, TId>(CancellationToken cancellationToken = default)
                    where TEntity : class
        {
            return await this.ToListAsync<TEntity, TId>(null, cancellationToken);
        }

        public async Task<List<TEntity>> ToListAsync<TEntity, TId>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var result = await this.GetCollection<TEntity>().FindAsync(filter, cancellationToken: cancellationToken);
            var entities = result.ToList();

            // TODO: implementar o AsNoTracking
            //Tracking(id, entity);

            var entitiesFromContext = entities
                .Select(entity => this.GetEntityFromContext<TEntity, TId>(entity, cancellationToken));

            return (await Task.WhenAll(entitiesFromContext)).ToList();
        }

        public async Task<TEntity> FirstOrDefaultAsync<TEntity, TId>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            //TODO: se filtro for pelo id
            //deve procurar na base de dados e se não encontrar procurar no contexto, mas poderia melhorar e procurar direto no contexto
            //fazer o teste com sqlite
            //add, firstordefault pelo id, sem salvar nada na base de dado e checar se retorna algo

            //var filterById = false;

            //if (filterById)
            //{
            //    var id = this.GetIdFromFilter(filter);
            //    return await this.FindAsync<TEntity>(id, cancellationToken);
            //}

            var result = await this.GetCollection<TEntity>().FindAsync(filter, cancellationToken: cancellationToken);
            var entity = result.FirstOrDefault();

            // TODO: implementar o AsNoTracking
            //Tracking(id, entity);

            if (entity == null || entity == default)
            {
                return default;
            }

            return await this.GetEntityFromContext<TEntity, TId>(entity, cancellationToken);
        }

        private object GetIdFromFilter<TEntity>(Expression<Func<TEntity, bool>> filter)
            where TEntity : class
        {
            //TODO:
            throw new NotImplementedException();
        }

        public async Task<TEntity> FindAsync<TEntity, TId>(TId id, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            return await this.GetEntityFromContext<TEntity, TId>(id, this.FindInDatabase<TEntity, TId>, cancellationToken);
        }

        private async Task<TEntity> FindInDatabase<TEntity, TId>(TId id, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var idFieldName = this.GetIdFieldName<TEntity>();
            var filter = Builders<TEntity>.Filter.Eq(idFieldName, id);

            var result = await this.GetCollection<TEntity>().FindAsync(filter, cancellationToken: cancellationToken);
            var entity = result.FirstOrDefault();

            return entity;
        }

        private async Task<TEntity> GetEntityFromContext<TEntity, TId>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            Func<TId, CancellationToken, Task<TEntity>> entityFactory = async (id, _) =>
            {
                return await Task.FromResult(entity);
            };

            var entityId = this.GetId<TEntity, TId>(entity);
            return await GetEntityFromContext<TEntity, TId>(entityId, entityFactory, cancellationToken);
        }

        private async Task<TEntity> GetEntityFromContext<TEntity, TId>(TId entityId, Func<TId, CancellationToken, Task<TEntity>> entityFactory, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var collectionName = this.GetCollectionName<TEntity>();

            Func<TId, Task<object>> entityFactoryWithTracking = async (id) =>
            {
                var entity = await entityFactory(id, cancellationToken);

                // TODO: implementar o AsNoTracking, tem que ver como é o comportamento no sqlite
                //não precisaria salvar o original, nem o collectionFromContext
                this.SaveAsOriginal(id, entity);

                return entity;
            };

            //TODO: verificar esse comportamento no EF SqlServer
            // Nao precisa verificar isso, assim é o comportamento do EF
            //var entitiesToRemove = this.collectionsToRemove.GetOrAdd(collectionName, (key) => new ConcurrentDictionary<object, object>());
            //var entityMarkedToRemove = entitiesToRemove.TryGetValue(id, out _);

            //if (entityMarkedToRemove)
            //{
            //    return null;
            //}

            var collectionFromContext = this.GetCollectionFromContext(collectionName);
            var entityFromContext = await GetOrAdd(entityId, collectionFromContext, entityFactoryWithTracking, cancellationToken);

            if (entityFromContext == null)
            {
                collectionFromContext.TryRemove(entityId, out var value);
            }

            return entityFromContext as TEntity;
        }

        //https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
        private async Task<object> GetOrAdd<TKey, TValue>(TKey key, ConcurrentDictionary<object, object> dictionay, Func<TKey, Task<TValue>> valueFactory, CancellationToken cancellationToken = default)
        {
            if (dictionay.TryGetValue(key, out var result))
            {
                return result;
            }

            await semaphore.WaitAsync(cancellationToken);

            try
            {
                //double-check
                if (dictionay.TryGetValue(key, out result))
                {
                    return result;
                }

                return dictionay[key] = await valueFactory(key);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void SaveAsOriginal<TEntity, TId>(TId id, TEntity entity)
            where TEntity : class
        {
            if (entity != null)
            {
                var collectionName = this.GetCollectionName<TEntity>();
                var idFieldName = this.GetIdFieldName(collectionName);

                var originals = this.GetCollectionOriginal(collectionName);
                var entitySerialized = new OriginalValue()
                {
                    ValueSerialized = this.Serialize(entity),
                    FilterByIdTyped = Builders<object>.Filter.Eq(idFieldName, (TId)id)
                };

                originals.TryAdd(id, entitySerialized);
            }
        }

        private TId GetId<TEntity, TId>(TEntity entity)
            where TEntity : class
        {
            // TODO: Id é o default, mas pode ser configurado outra propriedade para PK
            return (TId)typeof(TEntity).GetProperty("Id").GetValue(entity, null);
        }

        public TEntity Add<TEntity, TId>(TEntity entity)
            where TEntity : class
        {
            var collectionName = this.GetCollectionName<TEntity>();

            var id = this.GetId<TEntity, TId>(entity);

            var entities = this.GetCollectionFromContext(collectionName);
            var entitiesState = this.GetCollectionState(collectionName);

            entities[id] = entity;
            entitiesState[id] = EntityState.Added;

            // TODO: o EF deixa fazer dois Add para o mesmo id, então vamos seguir o comportamento
            //if (!entities.TryAdd(id, entity))
            //{
            //    throw new Exception("PrimaryKey violation");
            //}

            return entity;
        }

        public TEntity Update<TEntity, TId>(TEntity entity)
            where TEntity : class
        {
            //TODO: verificar se isso funciona sem antes fazer uma query, pois a entidade não vai estar no valor original, então talvez aqui seja interessante por

            var collectionName = this.GetCollectionName<TEntity>();

            var id = this.GetId<TEntity, TId>(entity);

            var entities = this.GetCollectionFromContext(collectionName);
            var entitiesState = this.GetCollectionState(collectionName);

            entities[id] = entity;
            entitiesState[id] = EntityState.Updated;

            // TODO: o EF deixa fazer dois Add para o mesmo id, então vamos seguir o comportamento
            //if (!entities.TryAdd(id, entity))
            //{
            //    throw new Exception("PrimaryKey violation");
            //}

            return entity;
        }

        public void Remove<TEntity, TId>(TEntity entity)
            where TEntity : class
        {
            var id = this.GetId<TEntity, TId>(entity);
            this.Remove<TEntity, TId>(id);
        }

        public void Remove<TEntity, TId>(object id)
            where TEntity : class
        {
            var collectionName = this.GetCollectionName<TEntity>();

            var entitiesState = this.GetCollectionState(collectionName);

            entitiesState[id] = EntityState.Removed;
        }

        private string Serialize(object entity)
        {
            // TODO: remover newtonsoft
            return Newtonsoft.Json.JsonConvert.SerializeObject(entity);
        }

        private TDictionary GetDictionaryByCollectionName<TDictionary>(string collectionName, ConcurrentDictionary<string, TDictionary> collections)
            where TDictionary : new()
        {
            return collections.GetOrAdd(collectionName, (key) => new TDictionary());
        }

        private ConcurrentDictionary<object, object> GetCollectionFromContext(string collectionName)
        {
            return this.GetDictionaryByCollectionName(collectionName, this.collectionsFromContext);
        }

        private ConcurrentDictionary<object, OriginalValue> GetCollectionOriginal(string collectionName)
        {
            return this.GetDictionaryByCollectionName(collectionName, this.collectionsOriginal);
        }

        private ConcurrentDictionary<object, EntityState> GetCollectionState(string collectionName)
        {
            return this.GetDictionaryByCollectionName(collectionName, this.collectionsState);
        }

        //TODO: configurewait e async em tudo
        //TODO: change to ValueTask

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //TODO: incluir uma transaction
            //https://docs.mongodb.com/manual/core/transactions/
            //https://docs.mongodb.com/manual/core/transactions-production-consideration/

            //TODO: pensar em como fazer um lock, ou lançar exception se chamar algum dos outros metodos

            var entitiesIdSavedByCollection = await AddAndRemoveChanges(cancellationToken);

            await UpdateChanges(entitiesIdSavedByCollection, cancellationToken);
        }

        //TODO: diminuir complexidade ciclomatica
        private async Task UpdateChanges(Dictionary<string, List<object>> entitiesIdSavedByCollection, CancellationToken cancellationToken = default)
        {
            foreach (var collection in this.collectionsFromContext)
            {
                //TODO: a ordem das operacoes faz diferenca? se fizer, temos que ordenar

                var collectionName = collection.Key;
                entitiesIdSavedByCollection.TryGetValue(collectionName, out var entitiesIdSaved);

                var entitiesToSave = collection.Value.Where(x => entitiesIdSaved == null || !entitiesIdSaved.Contains(x.Key));

                if (entitiesToSave.Any())
                {
                    var originalValues = this.GetCollectionOriginal(collectionName);

                    if (originalValues.Any())
                    {
                        var collectionDB = this.database.GetCollection<object>(collectionName);

                        foreach (var entity in entitiesToSave)
                        {
                            var id = entity.Key;
                            var data = entity.Value;

                            var hasOriginalValue = originalValues.TryGetValue(id, out var originalValue);

                            if (hasOriginalValue)
                            {
                                var shouldBeSave = this.ShouldBeSave(id, data, originalValue);

                                if (shouldBeSave)
                                {
                                    var idFieldName = this.GetIdFieldName(collectionName);
                                    var filter = originalValue.FilterByIdTyped;
                                    // TODO: pensar em ter o timestamp para concorrencia
                                    // TODO: pensar em ter created e updated para audit
                                    // TODO: pensar em ter createdby e updatedby para audit, necessario implementar IMongoAuditAuth

                                    var result = await collectionDB.ReplaceOneAsync(
                                                filter,
                                                data,
                                                new ReplaceOptions { IsUpsert = false },
                                                cancellationToken: cancellationToken);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<Dictionary<string, List<object>>> AddAndRemoveChanges(CancellationToken cancellationToken)
        {
            var entitiesIdSavedByCollection = new Dictionary<string, List<object>>();
            var entitiesToAdd = new List<object>();
            var entitiesToRemove = new List<object>();

            foreach (var collectionState in this.collectionsState)
            {
                var collectionName = collectionState.Key;
                var entitiesState = collectionState.Value.Where(x => x.Value == EntityState.Added || x.Value == EntityState.Removed);

                if (entitiesState.Any())
                {
                    var entitiesIdSaved = new List<object>();
                    var collectionDB = this.database.GetCollection<object>(collectionName);
                    var entitiesFromContext = new Lazy<ConcurrentDictionary<object, object>>(() => this.GetCollectionFromContext(collectionName));

                    foreach (var entityState in entitiesState)
                    {
                        var id = entityState.Key;
                        var state = entityState.Value;

                        if (state == EntityState.Added)
                        {
                            var exist = entitiesFromContext.Value.TryGetValue(id, out var entity);
                            if (exist)
                            {
                                entitiesIdSaved.Add(id);
                                entitiesToAdd.Add(entity);
                            }
                            else
                            {
                                //TODO: create specific exception
                                throw new Exception();
                            }
                        }
                        else if (state == EntityState.Removed)
                        {
                            entitiesIdSaved.Add(id);
                            entitiesToRemove.Add(id);
                        }
                    }

                    if (entitiesToAdd.Any())
                    {
                        await collectionDB.InsertManyAsync(entitiesToAdd, cancellationToken: cancellationToken);
                        entitiesToAdd.Clear();
                    }

                    if (entitiesToRemove.Any())
                    {
                        var idFieldName = this.GetIdFieldName(collectionName);
                        var removeByIds = Builders<object>.Filter.In(idFieldName, entitiesToRemove);
                        await collectionDB.DeleteManyAsync(removeByIds, cancellationToken: cancellationToken);
                        entitiesToRemove.Clear();
                    }

                    entitiesIdSavedByCollection.Add(collectionName, entitiesIdSaved);
                }
            }

            return entitiesIdSavedByCollection;
        }

        private bool ShouldBeSave(object id, object data, OriginalValue originalValue)
        {
            var dataSerialized = Serialize(data);
            var originalValueSerialized = originalValue.ValueSerialized;

            // TODO: remover newtonsoft
            var shouldBeSave = !Newtonsoft.Json.Linq.JToken.DeepEquals(originalValueSerialized, dataSerialized);

            originalValue.NewValueSerialized = dataSerialized;

            return shouldBeSave;
        }
    }

    public class OriginalValue
    {
        public OriginalValue()
        {
        }

        public string ValueSerialized { get; set; }

        public string NewValueSerialized { get; set; }

        public FilterDefinition<object> FilterByIdTyped { get; set; }
    }
}