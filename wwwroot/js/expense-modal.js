document.addEventListener('DOMContentLoaded', () => {
    const expenseModal = document.getElementById('expenseModal');
    if (!expenseModal) return;

    const expenseIdInput = document.getElementById('expenseId');
    const modalTitle = document.getElementById('expenseModalTitle');
    
    const form = document.getElementById('expenseForm');
    const amountInput = document.getElementById('expenseAmount');
    const btnSave = document.getElementById('btnSaveExpense');
    const btnText = btnSave.querySelector('.btn-text');
    const spinner = btnSave.querySelector('.spinner-border');
    const alertBox = document.getElementById('expenseModalAlert');

    // Proper modal initialization
    const modalElement = document.getElementById('expenseModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);

    // This handles opening the modal explicitly for creation
    document.querySelector('[data-bs-target="#expenseModal"]')?.addEventListener('click', () => {
        form.reset();
        form.action = '/Expenses/Create';
        expenseIdInput.value = '';
        modalTitle.textContent = 'Registrar Gasto';
        btnText.textContent = 'Añadir gasto';
    });

    expenseModal.addEventListener('shown.bs.modal', () => {
        amountInput.focus();
    });

    expenseModal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        form.action = '/Expenses/Create';
        expenseIdInput.value = '';
        modalTitle.textContent = 'Registrar Gasto';
        btnText.textContent = 'Añadir gasto';
        
        alertBox.classList.add('d-none');
        btnSave.disabled = false;
        btnText.classList.remove('d-none');
        spinner.classList.add('d-none');
        
        form.classList.remove('was-validated');
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        if (!form.checkValidity()) {
            e.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // Parse amount logic: replace comma with dot if necessary for model binder
        // Wait, MVC model binder for decimal usually respects culture.
        // We will send standard formData and MVC will parse based on culture.

        btnSave.disabled = true;
        btnText.classList.add('d-none');
        spinner.classList.remove('d-none');
        alertBox.classList.add('d-none');

        try {
            const formData = new FormData(form);
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            const result = await response.json();

            if (!response.ok || !result.succeeded) {
                throw new Error(result.errorMessage || 'Error al guardar el gasto.');
            }

            // PRIMERO: cerrar modal y restaurar botón/loading visual
            modal.hide();
            btnSave.disabled = false;
            btnText.classList.remove('d-none');
            spinner.classList.add('d-none');

            // DESPUÉS: Actualizaciones secundarias
            let uiUpdated = false;

            try {
                if (result.dashboardSummary && typeof updateDashboardUI === 'function') {
                    updateDashboardUI(result.dashboardSummary, result.warnings);
                    uiUpdated = true;
                } 
                
                if (typeof refreshPocketList === 'function') {
                    // refreshPocketList might be an async function, await if it returns a Promise
                    const refreshResult = refreshPocketList();
                    if (refreshResult instanceof Promise) {
                        await refreshResult;
                    }
                    uiUpdated = true;
                }
            } catch (secError) {
                console.error("Error secundario al actualizar la UI:", secError);
                // Si la UI falla, el modal ya se cerró y el gasto está guardado.
                // Podríamos mostrar un warning
                if (window.Toasts) Toasts.warning('Gasto guardado, pero falló la actualización visual.');
                return; // Cortar el flujo aquí para no hacer el toast normal
            }

            const successMsg = form.action.includes('Edit') ? 'Gasto actualizado correctamente' : 'Gasto guardado correctamente';

            if (uiUpdated) {
                if (window.Toasts) Toasts.success(successMsg);
            } else {
                sessionStorage.setItem('pf-toast-success', successMsg);
                window.location.reload();
            }

        } catch (error) {
            alertBox.textContent = error.message;
            alertBox.classList.remove('d-none');
        } finally {
            btnSave.disabled = false;
            btnText.classList.remove('d-none');
            spinner.classList.add('d-none');
        }
    });

    // --- LOGICA DE PAGOS A PLAZOS --- //
    const instForm = document.getElementById('installmentForm');
    if (instForm) {
        const instTotalAmount = document.getElementById('instTotalAmount');
        const instCount = document.getElementById('instCount');
        const instBaseAmount = document.getElementById('instBaseAmount');
        let userEditedBase = false;

        const calculateBase = () => {
            if (userEditedBase) return;
            const total = parseFloat(instTotalAmount.value) || 0;
            const count = parseInt(instCount.value) || 0;
            if (total > 0 && count > 1) {
                instBaseAmount.value = (total / count).toFixed(2);
            }
        };

        instTotalAmount.addEventListener('input', () => {
            userEditedBase = false;
            calculateBase();
        });
        
        instCount.addEventListener('input', () => {
            userEditedBase = false;
            calculateBase();
        });

        instBaseAmount.addEventListener('input', () => {
            userEditedBase = true;
        });

        const instBtnSave = document.getElementById('btnSaveInstallment');

        instForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            if (!instForm.checkValidity()) {
                e.stopPropagation();
                instForm.classList.add('was-validated');
                return;
            }

            instBtnSave.disabled = true;
            instBtnSave.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Procesando...';
            alertBox.classList.add('d-none');

            try {
                const formData = new FormData(instForm);
                const response = await fetch(instForm.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();

                if (!response.ok || !result.succeeded) {
                    throw new Error(result.errorMessage || 'Error al guardar el pago a plazos.');
                }

                // Cierra el modal solo en caso de éxito
                modal.hide();
                sessionStorage.setItem('pf-toast-success', 'Pago a plazos creado correctamente.');
                window.location.reload();

            } catch (error) {
                alertBox.textContent = error.message;
                alertBox.classList.remove('d-none');
            } finally {
                instBtnSave.disabled = false;
                instBtnSave.innerHTML = '<span class="btn-text">Añadir pago a plazos</span>';
            }
        });
    }
});
