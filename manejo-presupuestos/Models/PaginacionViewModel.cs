namespace manejo_presupuestos.Models
{
    public class PaginacionViewModel
    {
        private int recordsPorPagina = 10;
        private readonly int cantidadMaximaRecordsPorPagina = 50;
        public int Pagina { get; set; } = 1;
        public int RecordsPorSaltar => recordsPorPagina * (Pagina - 1);

        public int RecordsPorPagina
        {
            get
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = (value > cantidadMaximaRecordsPorPagina) ?
                    cantidadMaximaRecordsPorPagina :
                    value;
            }
        }

    }
}
