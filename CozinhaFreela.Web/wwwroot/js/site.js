// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", () => {
    const somenteNumeros = (valor, limite) =>
        valor.replace(/\D/g, "").slice(0, limite);

    const aplicarMascara = (campo) => {
        const tipo = campo.dataset.mask;

        if (tipo === "cpf") {
            const valor = somenteNumeros(campo.value, 11);
            campo.value = valor
                .replace(/^(\d{3})(\d)/, "$1.$2")
                .replace(/^(\d{3})\.(\d{3})(\d)/, "$1.$2.$3")
                .replace(/\.(\d{3})(\d)/, ".$1-$2");
        }

        if (tipo === "phone") {
            const valor = somenteNumeros(campo.value, 11);
            campo.value = valor.length <= 10
                ? valor
                    .replace(/^(\d{2})(\d)/, "($1) $2")
                    .replace(/(\d{4})(\d)/, "$1-$2")
                : valor
                    .replace(/^(\d{2})(\d)/, "($1) $2")
                    .replace(/(\d{5})(\d)/, "$1-$2");
        }

        if (tipo === "cep") {
            const valor = somenteNumeros(campo.value, 8);
            campo.value = valor.replace(/^(\d{5})(\d)/, "$1-$2");
        }

        if (tipo === "date") {
            const valor = somenteNumeros(campo.value, 8);
            campo.value = valor
                .replace(/^(\d{2})(\d)/, "$1/$2")
                .replace(/^(\d{2})\/(\d{2})(\d)/, "$1/$2/$3");
        }
    };

    document.querySelectorAll("[data-mask]").forEach((campo) => {
        aplicarMascara(campo);

        campo.addEventListener("input", () =>
            aplicarMascara(campo));
    });
});
