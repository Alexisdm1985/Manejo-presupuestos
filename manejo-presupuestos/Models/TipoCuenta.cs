﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace manejo_presupuestos.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:50, MinimumLength = 3, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre del tipo cuenta")]
        [Remote(action:"ExisteTipocuenta", controller:"TiposCuentas")]
        public string Nombre { get; set; }

        public int UsuarioId { get; set; }
        public int Orden { get; set; }

    }
}
