// Comportamientos compartidos por las pantallas de KioPlus.

(function () {
    "use strict";

    // Compara ignorando mayúsculas y tildes, para que "cafe" encuentre "Café"
    function normalizar(texto) {
        return (texto || "")
            .toLowerCase()
            .normalize("NFD")
            .replace(/[̀-ͯ]/g, "");
    }

    // ---------------------------------------------------------------------
    // Buscador con lista desplegable.
    //
    // Reemplaza al par "input de búsqueda + combo" por un solo control: se
    // escribe adentro y la lista se va filtrando; con el campo vacío muestra
    // todas las opciones, como un combo común. El valor elegido viaja en un
    // input oculto, así el formulario sigue enviando el id de siempre.
    // ---------------------------------------------------------------------
    function iniciarCombo(combo) {
        const entrada = combo.querySelector("[data-kp-combo-entrada]");
        const valor = combo.querySelector("[data-kp-combo-valor]");
        const lista = combo.querySelector("[data-kp-combo-lista]");
        if (!entrada || !valor || !lista) return;

        const opciones = Array.from(lista.querySelectorAll("[data-valor]"));
        const vacio = lista.querySelector("[data-kp-combo-vacio]");
        let resaltada = -1;

        // Al abrir el desplegable se ven todas las opciones. Recién se filtra
        // cuando el usuario escribe: si no, el texto de la opción ya elegida
        // actuaría de filtro y escondería al resto.
        let filtrando = false;

        function visibles() {
            return opciones.filter(o => !o.hidden);
        }

        function filtrar() {
            const buscado = filtrando ? normalizar(entrada.value) : "";
            opciones.forEach(o => {
                o.hidden = buscado !== "" && !normalizar(o.textContent).includes(buscado);
            });

            const hayResultados = visibles().length > 0;
            if (vacio) vacio.hidden = hayResultados;
            resaltar(hayResultados ? 0 : -1);
        }

        function resaltar(indice) {
            const disponibles = visibles();
            opciones.forEach(o => o.classList.remove("activa"));
            resaltada = indice;

            if (indice >= 0 && indice < disponibles.length) {
                disponibles[indice].classList.add("activa");
                disponibles[indice].scrollIntoView({ block: "nearest" });
            }
        }

        function abrir() {
            filtrar();
            lista.hidden = false;
        }

        function cerrar() {
            lista.hidden = true;
        }

        function elegir(opcion) {
            if (!opcion) return;
            filtrando = false;
            valor.value = opcion.dataset.valor;
            entrada.value = opcion.dataset.etiqueta || opcion.textContent.trim();
            cerrar();
            entrada.dispatchEvent(new Event("kp-combo-elegido", { bubbles: true }));
        }

        entrada.addEventListener("focus", function () {
            // Se muestran todas: lo que haya escrito es la opción ya elegida
            filtrando = false;
            abrir();
        });

        entrada.addEventListener("input", function () {
            // Mientras se escribe no hay selección válida: se elige de la lista
            valor.value = "";
            filtrando = true;
            abrir();
        });

        entrada.addEventListener("keydown", function (e) {
            if (e.key === "ArrowDown" || e.key === "ArrowUp") {
                e.preventDefault();
                if (lista.hidden) abrir();
                const total = visibles().length;
                if (total === 0) return;
                const paso = e.key === "ArrowDown" ? 1 : -1;
                resaltar((resaltada + paso + total) % total);
            } else if (e.key === "Enter") {
                if (!lista.hidden && resaltada >= 0) {
                    e.preventDefault();
                    elegir(visibles()[resaltada]);
                }
            } else if (e.key === "Escape") {
                cerrar();
            }
        });

        // El click se procesa antes que el blur, que se demora a propósito
        lista.addEventListener("mousedown", function (e) {
            const opcion = e.target.closest("[data-valor]");
            if (opcion) {
                e.preventDefault();
                elegir(opcion);
            }
        });

        entrada.addEventListener("blur", function () {
            window.setTimeout(cerrar, 120);
        });
    }

    // ---------------------------------------------------------------------
    // Ojo para mostrar u ocultar una contraseña
    // ---------------------------------------------------------------------
    // Con la clave oculta se ve el ojo abierto (tocarlo la muestra) y con la
    // clave a la vista se ve el ojo tachado (tocarlo la vuelve a ocultar).
    // Deja a la vista el ojo que corresponde al estado actual. Los dos display
    // se escriben siempre con un valor explícito: dejarlos vacíos devolvería la
    // decisión a la hoja de estilos, que esconde el segundo ojo, y entonces no
    // se vería ninguno de los dos.
    function pintarOjo(boton, oculta) {
        const abierto = boton.querySelector("[data-kp-ojo-abierto]");
        const tachado = boton.querySelector("[data-kp-ojo-tachado]");
        if (abierto) abierto.style.display = oculta ? "block" : "none";
        if (tachado) tachado.style.display = oculta ? "none" : "block";
        boton.setAttribute("aria-label", oculta ? "Mostrar contraseña" : "Ocultar contraseña");
    }

    // Ojo de un campo de contraseña: alterna el tipo del input.
    function iniciarVerClave(boton) {
        const campo = document.getElementById(boton.dataset.kpVerClave);
        if (!campo) return;

        const repintar = () => pintarOjo(boton, campo.type === "password");
        repintar();

        boton.addEventListener("click", function () {
            campo.type = campo.type === "password" ? "text" : "password";
            repintar();
            campo.focus();
        });
    }

    // Ojo de una contraseña que se muestra como texto en un listado: alterna
    // entre los puntos y el valor real. Usa el mismo ícono que el de los campos.
    function iniciarVerTexto(boton) {
        const celda = boton.parentElement.querySelector("[data-kp-texto-oculto]");
        if (!celda) return;

        const puntos = "●".repeat(8);
        const valor = celda.dataset.kpTextoOculto;

        const repintar = () => pintarOjo(boton, celda.textContent.trim() === puntos);
        celda.textContent = puntos;
        repintar();

        boton.addEventListener("click", function () {
            const oculta = celda.textContent.trim() === puntos;
            celda.textContent = oculta ? valor : puntos;
            repintar();
        });
    }

    // ---------------------------------------------------------------------
    // Período de los filtros con calendario
    // ---------------------------------------------------------------------
    // En histórico los calendarios quedan deshabilitados, así que no viajan en
    // el formulario y el filtro sale sin fechas límite. Si en modo rango se
    // vacían los dos calendarios, el período vuelve solo a histórico: borrar la
    // fecha deja de ser un error.
    function iniciarFiltroPeriodo(select) {
        const formulario = select.closest("form");
        if (!formulario) return;

        const fechas = Array.from(formulario.querySelectorAll("[data-kp-periodo-fecha]"));

        function pintar() {
            const historico = select.value !== "rango";
            fechas.forEach(f => {
                f.disabled = historico;
                if (historico) f.value = "";
            });
        }

        select.addEventListener("change", pintar);

        fechas.forEach(f => f.addEventListener("change", function () {
            if (fechas.every(x => x.value === "")) {
                select.value = "historico";
                pintar();
            }
        }));

        pintar();
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-kp-combo]").forEach(iniciarCombo);
        document.querySelectorAll("[data-kp-ver-clave]").forEach(iniciarVerClave);
        document.querySelectorAll("[data-kp-ver-texto]").forEach(iniciarVerTexto);
        document.querySelectorAll("[data-kp-periodo]").forEach(iniciarFiltroPeriodo);
    });
})();
