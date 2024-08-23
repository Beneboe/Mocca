using System.Threading.Tasks;
using MoccaProxy.Data;

namespace MoccaProxy.Interfaces;

public interface IMoccaRepository
{
    public Task AddAsync(MoccaRequest request, MoccaResponse response);

    public Task<MoccaResponse?> ResolveAsync(MoccaRequest request);
}