using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MODELO.seguridad
{

    public partial class Usuario
    {
        
        public Usuario()
        {
            Acciones = new HashSet<Accion>();
            Grupos = new HashSet<Grupo>();
        }
        [Key]
        public int USU_ID { get; set; }

        [StringLength(60)]
        public string USU_USUARIO { get; set; }

        [StringLength(64)]
        public string USU_CLAVE { get; set; }

        [StringLength(60)]
        public string USU_MAIL { get; set; }

        [ForeignKey("Estado_Usuario")]
        public int? EST_USU_ID { get; set; }

        public virtual Estado_Usuario Estado_Usuario { get; set; }


        public virtual ICollection<Accion> Acciones { get; set; }

        
        public virtual ICollection<Grupo> Grupos { get; set; }

        public bool AgregarAccion(Accion accion)
        {
            var ok = false;
            var ac = Acciones.FirstOrDefault(x=>x.ACC_ID == accion.ACC_ID);
            if(ac == null)
            {
                var accionGrupo = Grupos.FirstOrDefault(x=>x.Acciones.Contains(accion));
                if(accionGrupo == null)
                {
                    Acciones.Add(accion);
                    ok= true;
                }
            } 
            return ok;
        }


        public bool QuitarAccion(Accion accion)
        {
            var ok = false;
            var ac = Acciones.FirstOrDefault(x => x.ACC_ID == accion.ACC_ID);
            if (ac != null)
            {
                Acciones.Remove(accion);
                ok = true;

            }
            return ok;
        }


        public bool AgregarGrupo(Grupo grupo)
        {
            var grupoExistente = Grupos.FirstOrDefault(x => x.GRU_ID == grupo.GRU_ID);
            if (grupoExistente != null) return false;

            var accionesPersonalizadas = Acciones.Where(x => grupo.Acciones.Contains(x)).ToList();
            if (accionesPersonalizadas.Any())
            {
                foreach (var accion in accionesPersonalizadas)
                {
                    Acciones.Remove(accion);
                }
            }

            Grupos.Add(grupo);
            return true;
        }


        public bool QuitarGrupo(Grupo grupo)
        {
            var grupoExistente = Grupos.FirstOrDefault(x => x.GRU_ID == grupo.GRU_ID);
            if (grupoExistente == null) return false;
            else
            {
                Grupos.Remove(grupoExistente);
            }
            return true;
        }

        public ReadOnlyCollection<Grupo> getAllGruposActivos()
        {
            return Grupos.Where(x => x.Estado_Grupo.EST_GRU_NOMBRE != "Deshabilitado").ToList().AsReadOnly();
        }


        public ReadOnlyCollection<Accion> getAllAcciones()
        {
            return Acciones.ToList().AsReadOnly();
        }

    }
}
