async function translate(text: string) {
    var lang = document.querySelector("html").getAttribute("lang");
    var text = text.replace(/[\n\r]+/g, " ").replace(/&nbsp;/g, " ");
    return await fetch("https://libretranslate.com/translate", {
        method: "POST",
        body: JSON.stringify({
            q: text,
            source: "auto",
            target: lang
        }),
        headers: { "Content-Type": "application/json" }
    }).then(r => r.json())
        .then(j => j.translatedText);
}
$("#search").submit((_: any) => {
    if ($("#src").val() === "") {
        alert("Seleccione a lo menos 1 repositorio para la búsqueda");
        return false;
    }
    if ($("#q").val() === "") {
        alert("Ingrese algún termino para la búsqueda");
        return false;
    }
    else true;
});
//$(document).ready(function () {
//    $(".title").tooltip(function () {
//        $.post("/home/translate?text=" + $(this).html(), {},
//            function (response) {
//                $(this)
//                    .tooltip('hide')
//                    .attr('data-original-title', response)
//                    //.tooltip('fixTitle')
//                    .tooltip('show');
//            });
//    });
//    $(".title").mouseout(function () {
//        $(this).tooltip();
//        $('.ui-tooltip').hide();
//    });
//});
function empty() {
    var x = $("#fondos").val();
    if (x === "") {
        alert("Seleccione al menos una fuente de fondos concursables a buscar");
        return false;
    }
    var y = $("#estado").val();
    if (y === "") {
        alert("Seleccione al menos un estado de fondo para la búsqueda");
        return false;
    }
}