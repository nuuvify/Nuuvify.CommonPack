### IMPORTANTE: IBM DB2 até a versão 3.1.0.500 não é possivel usar SaveChangesAsync, pois causa exception ao fazer Add, apesar de funcionar com Update e Delete, dessa forma utilize SaveChanges (sincrono) em AppDbContext.cs

```csharp
            public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {

                var registries = SaveChanges();

                Debug.WriteLine($"SaveChanges executado com sucesso para {registries} registros, e {this.GetAggregatesChanges()} registros em entidades agregadas");

                return await Task.FromResult(registries);
            }
```