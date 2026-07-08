using Parkking.Models.Seguridad;
using Parkking.Repositories;

namespace Parkking.Services;

public class GrupoService
{
    private readonly GrupoRepository _repository;

    public GrupoService(GrupoRepository repository) => _repository = repository;

    public List<Grupo> GetAll() => _repository.GetAll();
    public Grupo? GetById(int id) => _repository.GetById(id);
    public List<Modulo> GetAllModulos() => _repository.GetAllModulos();

    public Grupo Create(Grupo grupo)
    {
        if (_repository.FindByNombre(grupo.GRU_NOMBRE) != null)
            throw new Exception("Ya existe el grupo " + grupo.GRU_NOMBRE + " en el sistema");

        _repository.Add(grupo);
        _repository.SaveChanges();
        return grupo;
    }

    public void Delete(Grupo grupo)
    {
        var existente = _repository.FindById(grupo.GRU_ID)
            ?? throw new Exception("El grupo " + grupo.GRU_NOMBRE + " no existe");
        _repository.Remove(existente);
        _repository.SaveChanges();
    }

    public void Update(Grupo grupo)
    {
        var existente = _repository.FindById(grupo.GRU_ID)
            ?? throw new Exception("El grupo " + grupo.GRU_NOMBRE + " no existe");

        existente.GRU_NOMBRE = grupo.GRU_NOMBRE;
        existente.EstadoGrupo = grupo.EstadoGrupo;
        _repository.SaveChanges();
    }
}
