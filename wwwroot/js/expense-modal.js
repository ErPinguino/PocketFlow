document.addEventListener('DOMContentLoaded', () => {
    const expenseModal = document.getElementById('expenseModal');
    if (!expenseModal) return;

    const modal = new bootstrap.Modal(expenseModal);
    const form = document.getElementById('expenseForm');
    const amountInput = document.getElementById('expenseAmount');
    const btnSave = document.getElementById('btnSaveExpense');
    const btnText = btnSave.querySelector('.btn-text');
    const spinner = btnSave.querySelector('.spinner-border');
    const alertBox = document.getElementById('expenseModalAlert');

    expenseModal.addEventListener('shown.bs.modal', () => {
        amountInput.focus();
    });

    expenseModal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        alertBox.classList.add('d-none');
        btnSave.disabled = false;
        btnText.classList.remove('d-none');
        spinner.classList.add('d-none');
        
        // Remove validation classes if any
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

            // Success, close modal
            modal.hide();

            // Check if we are on the dashboard to update it dynamically
            if (result.dashboardSummary && typeof updateDashboardUI === 'function') {
                updateDashboardUI(result.dashboardSummary, result.warnings);
            } else {
                // If we are in another page, just reload to see updates
                window.location.reload();
            }

        } catch (error) {
            alertBox.textContent = error.message;
            alertBox.classList.remove('d-none');
            
            btnSave.disabled = false;
            btnText.classList.remove('d-none');
            spinner.classList.add('d-none');
        }
    });
});
