
function InicializarFormularioTransacciones(urlObtenerCategorias) {

    $("#TipoOperacionId").change(async function () {

        const valorSeleccionado = $(this).val();

        const respuesta = await fetch(urlObtenerCategorias, {
            method: "POST",
            body: valorSeleccionado,
            headers: { 'Content-Type': 'application/json' }
        });

        const json = await respuesta.json();

        // Mapeo la respuesta para luego insertarla en el Select de categorias como <option>
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);

        $("#CategoriaId").html(opciones);

    });
};