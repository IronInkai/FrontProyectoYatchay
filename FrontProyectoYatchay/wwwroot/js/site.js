// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
<script>
    function seleccionarOpcion(id, element) {

        // limpiar selección previa
        document.querySelectorAll(".opcion-card")
            .forEach(el => el.classList.remove("selected"));

    // marcar nueva
    element.classList.add("selected");

    // guardar valor
    document.getElementById("opcionElegida").value = id;

    // activar botón
    document.getElementById("btnEnviar").disabled = false;
    }
</script>