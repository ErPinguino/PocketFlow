document.addEventListener('DOMContentLoaded', () => {
    
    // --- Variables de estado ---
    let currentCategory = new URLSearchParams(window.location.search).get('category') || '';
    let currentSearch = document.getElementById('expenseSearch')?.value || '';
    let currentSort = document.getElementById('expenseSort')?.value || 'newest';
    let currentPage = 1;

    // --- DOM Elements ---
    const searchInput = document.getElementById('expenseSearch');
    const sortSelect = document.getElementById('expenseSort');
    const filterButtons = document.querySelectorAll('.btn-filter');
    const listContainer = document.getElementById('expenseListContainer');
    
    const expenseModalEl = document.getElementById('expenseModal');
    const expenseModal = expenseModalEl ? new bootstrap.Modal(expenseModalEl) : null;
    
    const deleteModalEl = document.getElementById('deleteExpenseModal');
    const deleteModal = deleteModalEl ? new bootstrap.Modal(deleteModalEl) : null;
    const deleteForm = document.getElementById('deleteExpenseForm');
    const deleteExpenseId = document.getElementById('deleteExpenseId');
    const btnConfirmDelete = document.getElementById('btnConfirmDelete');
    const deleteAlert = document.getElementById('deleteExpenseAlert');

    // --- Refrescar Lista (AJAX) ---
    window.refreshPocketList = async function(page = 1) {
        currentPage = page;
        
        try {
            // Construir URL
            const url = new URL('/Pocket/ExpenseListPartial', window.location.origin);
            if (currentCategory) url.searchParams.append('category', currentCategory);
            if (currentSearch) url.searchParams.append('search', currentSearch);
            if (currentSort) url.searchParams.append('sort', currentSort);
            url.searchParams.append('page', currentPage);

            // Sprint 7: Check if we are in History view to pass planId
            const historyMatch = window.location.pathname.match(/\/History\/Detail\/([a-fA-F0-9-]+)/i);
            if (historyMatch && historyMatch[1]) {
                url.searchParams.append('planId', historyMatch[1]);
            }

            // Fetch partial view
            const response = await fetch(url);
            if (response.ok) {
                const html = await response.text();
                listContainer.innerHTML = html;
                attachListEventHandlers(); // re-vincular eventos a los nuevos botones
            }
        } catch (error) {
            console.error('Error al actualizar la lista de gastos:', error);
        }
    };

    // --- Event Handlers de Filtros, Búsqueda, Ordenación ---
    
    // Filtros por categoría
    filterButtons.forEach(btn => {
        btn.addEventListener('click', (e) => {
            filterButtons.forEach(b => {
                b.classList.remove('btn-primary');
                b.classList.add('btn-outline-primary');
            });
            e.currentTarget.classList.remove('btn-outline-primary');
            e.currentTarget.classList.add('btn-primary');
            
            currentCategory = e.currentTarget.dataset.category;
            refreshPocketList(1);
        });
    });

    // Búsqueda (Debounce)
    let searchTimeout;
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                currentSearch = e.target.value;
                refreshPocketList(1);
            }, 300);
        });
    }

    // Ordenación
    if (sortSelect) {
        sortSelect.addEventListener('change', (e) => {
            currentSort = e.target.value;
            refreshPocketList(1);
        });
    }

    // --- Vinculación dinámica (Edición / Borrado / Paginación) ---
    function attachListEventHandlers() {
        // Paginación
        const pageButtons = document.querySelectorAll('.btn-page');
        pageButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const page = e.currentTarget.dataset.page;
                refreshPocketList(page);
            });
        });

        // Abrir Modal Edición
        const editButtons = document.querySelectorAll('.btn-edit-expense');
        editButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                const data = e.currentTarget.dataset;
                
                // Rellenar Modal
                document.getElementById('expenseId').value = data.id;
                document.getElementById('expenseAmount').value = data.amount;
                document.getElementById('expenseDesc').value = data.desc;
                
                if (data.category === 'Life') {
                    document.getElementById('catLife').checked = true;
                } else {
                    document.getElementById('catWhim').checked = true;
                }

                const form = document.getElementById('expenseForm');
                form.action = '/Expenses/Edit';

                document.getElementById('expenseModalTitle').textContent = 'Editar Gasto';
                document.getElementById('expenseBtnText').textContent = 'Guardar cambios';

                expenseModal.show();
            });
        });

        // Abrir Modal Eliminación
        const deleteButtons = document.querySelectorAll('.btn-delete-expense');
        deleteButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                const data = e.currentTarget.dataset;
                deleteExpenseId.value = data.id;
                deleteAlert.classList.add('d-none');
                deleteModal.show();
            });
        });
    }

    // --- Envío de Borrado (AJAX) ---
    if (deleteForm) {
        deleteForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const btnText = btnConfirmDelete.querySelector('.btn-text');
            const spinner = btnConfirmDelete.querySelector('.spinner-border');
            
            btnConfirmDelete.disabled = true;
            btnText.classList.add('d-none');
            spinner.classList.remove('d-none');
            deleteAlert.classList.add('d-none');

            try {
                const formData = new FormData(deleteForm);
                const response = await fetch(deleteForm.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();

                if (!response.ok || !result.succeeded) {
                    throw new Error(result.errorMessage || 'Error al eliminar el gasto.');
                }

                // Éxito
                deleteModal.hide();
                
                if (result.dashboardSummary && typeof updateDashboardUI === 'function') {
                    updateDashboardUI(result.dashboardSummary, []);
                }
                
                refreshPocketList(currentPage);

            } catch (error) {
                deleteAlert.textContent = error.message;
                deleteAlert.classList.remove('d-none');
            } finally {
                btnConfirmDelete.disabled = false;
                btnText.classList.remove('d-none');
                spinner.classList.add('d-none');
            }
        });
    }

    // --- Función para actualizar los resúmenes del Pocket ---
    // El modal de Expenses (crear/editar) devuelve result.dashboardSummary y actualiza todo globalmente.
    // Nosotros sobreescribimos updateDashboardUI si existe, o la implementamos aquí, para actualizar los 
    // bloques del Pocket (Disponible, Semanal, Vida, Capricho) además del Dashboard.
    
    // Hacer un interceptor/extensión de updateDashboardUI
    const originalUpdateDashboardUI = window.updateDashboardUI;
    window.updateDashboardUI = function(summary, warnings) {
        // Ejecutar original si existe
        if (typeof originalUpdateDashboardUI === 'function') {
            originalUpdateDashboardUI(summary, warnings);
        }

        // Actualizar UI del Pocket si estamos en la vista Pocket
        // Buscamos los elementos del DOM (h5 elements in dashboard-card)
        const pocketCards = document.querySelectorAll('.dashboard-card h5');
        if (pocketCards.length >= 4 && summary) {
            const formatCurrency = (amount) => new Intl.NumberFormat(
                summary.currency === "EUR" ? "es-ES" : "en-US", 
                { style: 'currency', currency: summary.currency }
            ).format(amount);

            // Disponible
            pocketCards[0].textContent = formatCurrency(summary.freePocketRemaining);
            pocketCards[0].className = 'fw-bold ' + (summary.freePocketRemaining < 0 ? 'text-danger' : 'text-success');

            // Semanal
            pocketCards[1].textContent = formatCurrency(summary.weeklyRemaining);
            pocketCards[1].className = 'fw-bold ' + (summary.weeklyRemaining < 0 ? 'text-danger' : '');

            // Vida
            pocketCards[2].textContent = formatCurrency(summary.lifeRemaining);
            pocketCards[2].className = 'fw-bold ' + (summary.lifeRemaining < 0 ? 'text-danger' : '');

            // Caprichos
            pocketCards[3].textContent = formatCurrency(summary.whimRemaining);
            pocketCards[3].className = 'fw-bold ' + (summary.whimRemaining < 0 ? 'text-danger' : '');
        }
    };

    // Inicializar eventos de la lista renderizada desde servidor (para vista Bolsillo)
    attachListEventHandlers();

    // Sprint 7: Si estamos en histórico, la lista viene vacía y tenemos que cargarla
    if (window.location.pathname.match(/\/History\/Detail\//i)) {
        refreshPocketList(1);
    }
});
