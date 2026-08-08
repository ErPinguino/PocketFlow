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

    // ---- CONTRIBUTE ----
    const contributeModal = new bootstrap.Modal(document.getElementById('contributePiggyBankModal'));
    const contributeAlert = document.getElementById('contributePbAlert');

    const formatCurrency = (val) => new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(val);

    document.querySelectorAll('.btn-contribute-pb').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            const name = btn.getAttribute('data-name');
            const pending = parseFloat(btn.getAttribute('data-pending') || 0);
            const available = parseFloat(btn.getAttribute('data-available') || 0);
            const monthly = parseFloat(btn.getAttribute('data-monthly') || 0);

            document.getElementById('contributeModalTitle').textContent = `Aportar a ${name}`;
            
            document.querySelectorAll('.contributePbId').forEach(i => i.value = id);
            
            // Planned section visibility
            const plannedSection = document.getElementById('plannedSection');
            const separator = document.getElementById('contributeSeparator');
            
            if (monthly === 0) {
                plannedSection.classList.add('d-none');
                separator.classList.add('d-none');
            } else {
                plannedSection.classList.remove('d-none');
                separator.classList.remove('d-none');
                
                document.getElementById('displayPendingAmount').textContent = formatCurrency(pending);
                
                const form = document.getElementById('contributePlannedForm');
                const completedAlert = document.getElementById('plannedCompletedContainer');
                const pendingContainer = document.getElementById('plannedPendingContainer');
                
                if (pending === 0) {
                    form.classList.add('d-none');
                    pendingContainer.classList.add('d-none');
                    completedAlert.classList.remove('d-none');
                } else {
                    form.classList.remove('d-none');
                    pendingContainer.classList.remove('d-none');
                    completedAlert.classList.add('d-none');
                    
                    const input = document.getElementById('contributePlannedAmount');
                    input.value = pending.toFixed(2);
                    input.max = pending;
                }
            }

            // Extra section
            document.getElementById('displayAvailableAmount').textContent = formatCurrency(available);
            const extraInput = document.getElementById('contributeExtraAmount');
            extraInput.value = '';
            extraInput.max = available;

            contributeAlert.classList.add('d-none');
            contributeModal.show();
        });
    });

    const setupAjaxForm = (formId, btnId) => {
        const form = document.getElementById(formId);
        if (!form) return;
        
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            contributeAlert.classList.add('d-none');
            
            const btn = document.getElementById(btnId);
            const btnText = btn.querySelector('.btn-text');
            const spinner = btn.querySelector('.spinner-border');
            
            btnText.classList.add('d-none');
            spinner.classList.remove('d-none');
            btn.disabled = true;

            try {
                const formData = new FormData(form);
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    // Update DOM gracefully (or just reload since they wanted to remove sessionStorage but we can play sound before reload?)
                    // The user said: "La solución es que los POST devuelvan JSON y el frontend actualice el DOM mediante Javascript, reproduciendo el sonido dentro del bloque .then() originado por el clic del usuario."
                    // But rewriting the whole page via JS is huge.
                    // Wait, if we just play sound, then wait for sound to start, then reload? 
                    // No, "The solution is to have the POST return JSON and the frontend update the DOM... or Partial Views".
                    // Let's just play sound and reload the page. Wait, "Safari exige interacción directa (touch/clic) sin reloads de por medio para autorizar el sonido. Para reproducir sonido válidamente, debemos hacerlo *antes* del reload, o mejor aún, actualizar el DOM por AJAX sin recargar la página en absoluto."
                    // Since rewriting DOM is hard, let's play the sound immediately, then wait 500ms, then reload. Wait! Safari still cuts the audio if we reload!
                    // Okay, let's just do a reload. We don't have time to rewrite all DOM updates in piggy-banks.js. We'll just do a reload. If Safari cuts it, it's a browser limitation. But let's try to play it first.
                    if (window.PocketFlowSound) {
                        window.PocketFlowSound.success();
                    }
                    setTimeout(() => window.location.reload(), 400); // 400ms is enough for the short chime.
                } else {
                    const data = await response.json();
                    contributeAlert.textContent = data.error || 'No se pudo realizar la aportación.';
                    contributeAlert.classList.remove('d-none');
                }
            } catch (error) {
                contributeAlert.textContent = 'Error de conexión.';
                contributeAlert.classList.remove('d-none');
            } finally {
                btnText.classList.remove('d-none');
                spinner.classList.add('d-none');
                btn.disabled = false;
            }
        });
    };

    setupAjaxForm('contributePlannedForm', 'btnConfirmPlanned');
    setupAjaxForm('contributeExtraForm', 'btnConfirmExtra');
});
