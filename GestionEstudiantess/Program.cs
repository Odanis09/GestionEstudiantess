using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEstudiantes
{

    public class ResultadoOperacion<T>
    {
        public string Mensaje { get; set; }
        public bool Exito { get; set; }
        public T Datos { get; set; }

        public ResultadoOperacion(bool exito, string mensaje, T datos = default)
        {
            Exito = exito;
            Mensaje = mensaje;
            Datos = datos;
        }


    }

    public abstract class Estudiante
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<double> CalificacionesExamen { get; } = new List<double>();
        public List<double> CalifcacionesPractica { get; } = new List<double>();


        protected Estudiante(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public virtual double CalcularNotaFinal()
        {
            double promExam = CalificacionesExamen.Any() ? CalificacionesExamen.Average() : 0;
            double promPract = CalifcacionesPractica.Any() ? CalifcacionesPractica.Average() : 0;
            return 0.6 * promExam + 0.4 * promPract;

        }

        public virtual void MostrarInfo()
        {
            Console.WriteLine($"{Id}\t{Nombre}\tFinal: {CalcularNotaFinal():F2}");
        }
    }

    public class EstudiantePresencial : Estudiante
    {
        public string Campus { get; set; }
        public EstudiantePresencial(int id, string nombre, string campus)
                : base(id, nombre)
        {
            Campus = campus;
        }
        public override void MostrarInfo()
        {
            base.MostrarInfo();
            Console.WriteLine($"  [Presencial en {Campus}]");
        }
    }

    public class Grupo
    {
        public string NombreGrupo { get; set; }
        private readonly List<Estudiante> estudiantes = new List<Estudiante>();
        public Grupo(string nombreGrupo) { NombreGrupo = nombreGrupo; }

        public ResultadoOperacion<Estudiante> AgregarEstudiante(Estudiante estudiante)
        {
            if (estudiantes.Exists(e => e.Id == estudiante.Id))
                return new ResultadoOperacion<Estudiante>(false, "El estudiante ya está en el grupo.");
            estudiantes.Add(estudiante);
            return new ResultadoOperacion<Estudiante>(true, "Estudiante agregado.", estudiante);
        }
        public void RegistrarExamen(int idEst, double nota)
            => estudiantes.Find(e => e.Id == idEst)?.CalificacionesExamen.Add(nota);
        public void RegistrarPractica(int idEst, double nota)
            => estudiantes.Find(e => e.Id == idEst)?.CalifcacionesPractica.Add(nota);

        public void MostrarCalificaciones()
        {
            Console.WriteLine($"\nGrupo: {NombreGrupo}");
            Console.WriteLine("ID\tNombre\tNota Final");
            estudiantes.ForEach(e => e.MostrarInfo());
        }

        public double PorcentajeAprobados()
        {
            if (!estudiantes.Any()) return 0;
            int aprobados = estudiantes.Count(e => e.CalcularNotaFinal() >= 70);
            return 100.0 * aprobados / estudiantes.Count;
        }

    }

    public class Asignatura
    {
        public string NombreAsignatura { get; set; }
        private readonly List<Grupo> grupos = new List<Grupo>();
        public Asignatura(string nombre) { NombreAsignatura = nombre; }

        public ResultadoOperacion<Grupo> CrearGrupo(string nombreGrupo)
        {
            if (grupos.Exists(g => g.NombreGrupo == nombreGrupo))
                return new ResultadoOperacion<Grupo>(false, "El grupo ya existe.");
            var grupo = new Grupo(nombreGrupo);
            grupos.Add(grupo);
            return new ResultadoOperacion<Grupo>(true, "Grupo creado.", grupo);
        }

        public Grupo ObtenerGrupo(string nombreGrupo)
            => grupos.Find(g => g.NombreGrupo == nombreGrupo);
    }


    public class Docente
    {
        public string Nombre { get; set; }
        private readonly List<Asignatura> asignaturas = new List<Asignatura>();
        public Docente(string nombre) { Nombre = nombre; }

        public ResultadoOperacion<Asignatura> AgregarAsignatura(string nombreAsignatura)
        {
            if (asignaturas.Exists(a => a.NombreAsignatura == nombreAsignatura))
                return new ResultadoOperacion<Asignatura>(false, "Asignatura ya asignada.");
            var asign = new Asignatura(nombreAsignatura);
            asignaturas.Add(asign);
            return new ResultadoOperacion<Asignatura>(true, "Asignatura agregada.", asign);
        }

        public Asignatura ObtenerAsignatura(string nombre)
            => asignaturas.Find(a => a.NombreAsignatura == nombre);
    }


    public static class InterfazConsola
    {
        public static void Ejecutar()
        {
            var docente = CrearDocente();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Gestión de Estudiantes ---");
                Console.WriteLine("1. Agregar asignatura");
                Console.WriteLine("2. Crear grupo en asignatura");
                Console.WriteLine("3. Agregar estudiante a grupo");
                Console.WriteLine("4. Registrar calificación");
                Console.WriteLine("5. Mostrar calificaciones y porcentaje aprobados");
                Console.WriteLine("0. Salir");
                Console.Write("Opción: ");
                var opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1": AgregarAsignatura(docente); break;
                    case "2": CrearGrupo(docente); break;
                    case "3": AgregarEstudiante(docente); break;
                    case "4": RegistrarCalificacion(docente); break;
                    case "5": MostrarReporte(docente); break;
                    case "0": return;
                    default: Console.WriteLine("Opción inválida."); Pausa(); break;
                }
            }
        }

        private static Docente CrearDocente()
        {
            Console.Write("Nombre del docente: ");
            return new Docente(Console.ReadLine());
        }

        private static void AgregarAsignatura(Docente docente)
        {
            Console.Write("Nombre asignatura: ");
            var res = docente.AgregarAsignatura(Console.ReadLine());
            Console.WriteLine(res.Mensaje);
            Pausa();
        }

        private static void CrearGrupo(Docente docente)
        {
            Console.Write("Asignatura: ");
            var asig = docente.ObtenerAsignatura(Console.ReadLine());
            if (asig == null) { Console.WriteLine("Asignatura no encontrada."); Pausa(); return; }
            Console.Write("Nombre grupo: ");
            Console.WriteLine(asig.CrearGrupo(Console.ReadLine()).Mensaje);
            Pausa();
        }

        private static void AgregarEstudiante(Docente docente)
        {
            Console.Write("Asignatura: "); var asig = docente.ObtenerAsignatura(Console.ReadLine());
            if (asig == null) { Console.WriteLine("Asignatura no encontrada."); Pausa(); return; }
            Console.Write("Grupo: "); var grp = asig.ObtenerGrupo(Console.ReadLine());
            if (grp == null) { Console.WriteLine("Grupo no existe."); Pausa(); return; }

            Console.Write("ID estudiante: "); int id = int.Parse(Console.ReadLine());
            Console.Write("Nombre estudiante: "); string nom = Console.ReadLine();
            Console.Write("Tipo (1=Presencial,2=Distancia): "); bool esPres = Console.ReadLine() == "1";
            Estudiante est = esPres
                ? new EstudiantePresencial(id, nom, Prompt("Campus: "))
                : new EstudianteDistancia(id, nom, Prompt("Zona horaria: "));
            Console.WriteLine(grp.AgregarEstudiante(est).Mensaje);
            Pausa();
        }

        private static void RegistrarCalificacion(Docente docente)
        {
            Console.Write("Asignatura: "); var asig = docente.ObtenerAsignatura(Console.ReadLine());
            if (asig == null) { Console.WriteLine("Asignatura no encontrada."); Pausa(); return; }
            Console.Write("Grupo: "); var grp = asig.ObtenerGrupo(Console.ReadLine());
            if (grp == null) { Console.WriteLine("Grupo no existe."); Pausa(); return; }

            Console.Write("ID estudiante: "); int id = int.Parse(Console.ReadLine());
            Console.Write("Tipo calificación (1=Examen,2=Práctica): "); bool esExam = Console.ReadLine() == "1";
            Console.Write("Nota: "); double nota = double.Parse(Console.ReadLine());
            if (esExam) grp.RegistrarExamen(id, nota); else grp.RegistrarPractica(id, nota);
            Console.WriteLine("Calificación registrada.");
            Pausa();
        }

        private static void MostrarReporte(Docente docente)
        {
            Console.Write("Asignatura: "); var asig = docente.ObtenerAsignatura(Console.ReadLine());
            if (asig == null) { Console.WriteLine("Asignatura no encontrada."); Pausa(); return; }
            Console.Write("Grupo: "); var grp = asig.ObtenerGrupo(Console.ReadLine());
            if (grp == null) { Console.WriteLine("Grupo no existe."); Pausa(); return; }
            grp.MostrarCalificaciones();
            Console.WriteLine($"Porcentaje aprobados: {grp.PorcentajeAprobados():F2}%\n");
            Pausa();
        }

        private static string Prompt(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }

        private static void Pausa()
        {
            Console.WriteLine("Presione Enter para continuar...");
            Console.ReadLine();
        }
    }


    public class Programa
    {
        public static void Main()
        {
            InterfazConsola.Ejecutar();
        }
    }
}