
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_Nuuvify.CommonPack&metric=alert_status)](https://sonarcloud.io/project/overview?id=nuuvify_Nuuvify.CommonPack)

```csharp
            public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {

                var registries = SaveChanges();

                Debug.WriteLine($"SaveChanges executado com sucesso para {registries} registros, e {this.GetAggregatesChanges()} registros em entidades agregadas");

                return await Task.FromResult(registries);
            }
```
