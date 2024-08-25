using Mocca.Data;

namespace Mocca.Interfaces;

public interface IMoccaRepository
{
    public Task AddAsync(MoccaRequest request, MoccaResponse response);

    public Task<MoccaResponse?> ResolveAsync(MoccaRequest request);
}