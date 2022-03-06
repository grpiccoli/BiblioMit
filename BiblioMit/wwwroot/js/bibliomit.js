var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
function translate(text) {
    var text;
    return __awaiter(this, void 0, void 0, function* () {
        var lang = document.querySelector("html").getAttribute("lang");
        text = text.replace(/[\n\r]+/g, " ").replace(/&nbsp;/g, " ");
        return yield fetch("https://libretranslate.com/translate", {
            method: "POST",
            body: JSON.stringify({
                q: text,
                source: "auto",
                target: lang
            }),
            headers: { "Content-Type": "application/json" }
        }).then(r => r.json())
            .then(j => j.translatedText);
    });
}
$("#search").submit((_) => {
    if ($("#src").val() === "") {
        alert("Seleccione a lo menos 1 repositorio para la búsqueda");
        return false;
    }
    if ($("#q").val() === "") {
        alert("Ingrese algún termino para la búsqueda");
        return false;
    }
    else
        true;
});
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
//# sourceMappingURL=bibliomit.js.map