document.addEventListener('DOMContentLoaded', () => {
    // ---- CREATE ----
    const createForm = document.getElementById('createPbForm');
    const createAlert = document.getElementById('createPbAlert');
    const btnCreatePb = document.getElementById('btnCreatePb');

    if (createForm) {
        createForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            createAlert.classList.add('d-none');
            
            const btnText = btnCreatePb.querySelector('.btn-text');
            const spinner = btnCreatePb.querySelector('.spinner-border');
            
            btnText.classList.add('d-none');
            spinner.classList.remove('d-none');
            btnCreatePb.disabled = true;

            try {
                const formData = new FormData(createForm);
                const response = await fetch(createForm.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    sessionStorage.setItem('pf-toast-success', 'Hucha creada correctamente');
                    window.location.reload();
                } else {
                    const result = await response.json();
                    createAlert.textContent = result.error || 'Error al crear la hucha.';
                    createAlert.classList.remove('d-none');
                }
            } catch (error) {
                createAlert.textContent = 'Error de conexión.';
                createAlert.classList.remove('d-none');
            } finally {
                btnText.classList.remove('d-none');
                spinner.classList.add('d-none');
                btnCreatePb.disabled = false;
            }
        });
    }

    // ---- EDIT ----
    const editModal = new bootstrap.Modal(document.getElementById('editPiggyBankModal'));
    const editForm = document.getElementById('editPbForm');
    const editAlert = document.getElementById('editPbAlert');
    const btnEditPb = document.getElementById('btnEditPb');

    document.querySelectorAll('.btn-edit-pb').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            
            try {
                const response = await fetch(`/PiggyBanks/GetForEdit/${id}`);
                if (response.ok) {
                    const data = await response.json();
                    
                    document.getElementById('editId').value = data.id;
                    document.getElementById('editIcon').value = data.icon || '';
                    document.getElementById('editName').value = data.name;
                    document.getElementById('editTargetAmount').value = data.targetAmount;
                    document.getElementById('editMonthlyContribution').value = data.monthlyContribution;
                    
                    editAlert.classList.add('d-none');
                    editModal.show();
                } else {
                    if (window.Toasts) Toasts.error('No se pudo cargar la información de la hucha.');
                }
            } catch (error) {
                if (window.Toasts) Toasts.error('Error de conexión.');
            }
        });
    });

    if (editForm) {
        editForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            editAlert.classList.add('d-none');
            
            const btnText = btnEditPb.querySelector('.btn-text');
            const spinner = btnEditPb.querySelector('.spinner-border');
            
            btnText.classList.add('d-none');
            spinner.classList.remove('d-none');
            btnEditPb.disabled = true;

            try {
                const formData = new FormData(editForm);
                const response = await fetch(editForm.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    sessionStorage.setItem('pf-toast-success', 'Hucha actualizada correctamente');
                    window.location.reload();
                } else {
                    const result = await response.json();
                    editAlert.textContent = result.error || 'Error al actualizar la hucha.';
                    editAlert.classList.remove('d-none');
                }
            } catch (error) {
                editAlert.textContent = 'Error de conexión.';
                editAlert.classList.remove('d-none');
            } finally {
                btnText.classList.remove('d-none');
                spinner.classList.add('d-none');
                btnEditPb.disabled = false;
            }
        });
    }

    // ---- ARCHIVE ----
    const archiveModal = new bootstrap.Modal(document.getElementById('archivePiggyBankModal'));
    const archiveForm = document.getElementById('archivePbForm');
    const archiveAlert = document.getElementById('archivePbAlert');
    const btnConfirmArchive = document.getElementById('btnConfirmArchive');

    document.querySelectorAll('.btn-archive-pb').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            document.getElementById('archivePbId').value = id;
            archiveAlert.classList.add('d-none');
            archiveModal.show();
        });
    });

    if (archiveForm) {
        archiveForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            archiveAlert.classList.add('d-none');
            
            const btnText = btnConfirmArchive.querySelector('.btn-text');
            const spinner = btnConfirmArchive.querySelector('.spinner-border');
            
            btnText.classList.add('d-none');
            spinner.classList.remove('d-none');
            btnConfirmArchive.disabled = true;

            try {
                const formData = new FormData(archiveForm);
                const response = await fetch(archiveForm.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    sessionStorage.setItem('pf-toast-success', 'Hucha archivada correctamente');
                    window.location.reload();
                } else {
                    const data = await response.json();
                    archiveAlert.textContent = data.error || 'No se pudo archivar.';
                    archiveAlert.classList.remove('d-none');
                }
            } catch (error) {
                archiveAlert.textContent = 'Error de conexión.';
                archiveAlert.classList.remove('d-none');
            } finally {
                btnText.classList.remove('d-none');
                spinner.classList.add('d-none');
                btnConfirmArchive.disabled = false;
            }
        });
    }

    // ---- REACTIVATE ----
    const reactivateModal = new bootstrap.Modal(document.getElementById('reactivatePiggyBankModal'));
    const reactivateForm = document.getElementById('reactivatePbForm');
    const reactivateAlert = document.getElementById('reactivatePbAlert');
    const btnConfirmReactivate = document.getElementById('btnConfirmReactivate');

    document.querySelectorAll('.btn-reactivate-pb').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            const name = btn.getAttribute('data-name');
            document.getElementById('reactivatePbId').value = id;
            document.getElementById('reactivateMessage').textContent = `La hucha "${name}" volverá a recibir aportaciones a partir del próximo mes.`;
            reactivateAlert.classList.add('d-none');
            reactivateModal.show();
        });
    });

    if (reactivateForm) {
        reactivateForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            reactivateAlert.classList.add('d-none');
            
            const btnText = btnConfirmReactivate.querySelector('.btn-text');
            const spinner = btnConfirmReactivate.querySelector('.spinner-border');
            
            btnText.classList.add('d-none');
            spinner.classList.remove('d-none');
            btnConfirmReactivate.disabled = true;

            try {
                const formData = new FormData(reactivateForm);
                const response = await fetch(reactivateForm.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    sessionStorage.setItem('pf-toast-success', 'Hucha reactivada correctamente');
                    window.location.reload();
                } else {
                    const data = await response.json();
                    reactivateAlert.textContent = data.error || 'No se pudo reactivar.';
                    reactivateAlert.classList.remove('d-none');
                }
            } catch (error) {
                reactivateAlert.textContent = 'Error de conexión.';
                reactivateAlert.classList.remove('d-none');
            } finally {
                btnText.classList.remove('d-none');
                spinner.classList.add('d-none');
                btnConfirmReactivate.disabled = false;
            }
        });
    }
});
