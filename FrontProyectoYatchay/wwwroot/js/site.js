// site.js
function seleccionarOpcion(id, element) {  

    // limpiar selección previa
    document.querySelectorAll('.opcion-card')
        .forEach(c => c.classList.remove('border-primary', 'bg-light'));

    // marcar nueva
    element.classList.add('border-primary', 'bg-light');

    // guardar valor
    const input = document.getElementById("opcionElegida");
    input.value = id;

    // activar botón
    document.getElementById("btnEnviar").disabled = false;
}
