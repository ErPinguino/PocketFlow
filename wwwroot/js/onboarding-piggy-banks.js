document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("piggy-banks-container");
    const addBtn = document.getElementById("add-piggy-bank-btn");

    if (!container || !addBtn) return;

    // Use a counter based on current time to avoid index collisions when removing/adding
    let counter = container.children.length;

    addBtn.addEventListener("click", function () {
        const index = counter++; 
        
        const card = document.createElement("div");
        card.className = "pb-card mb-3";
        card.innerHTML = `
            <button type="button" class="btn btn-sm btn-outline-danger pb-delete-btn">Eliminar</button>
            <input type="hidden" name="PiggyBanks.Index" value="${index}" />
            
            <div class="mb-3">
                <label class="form-label">Nombre</label>
                <input name="PiggyBanks[${index}].Name" class="form-control" placeholder="Ej: Viaje a Japón" required />
            </div>
            
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label class="form-label">Objetivo total</label>
                    <input type="number" step="0.01" name="PiggyBanks[${index}].TargetAmount" class="form-control" required />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Ahorrado actualmente</label>
                    <input type="number" step="0.01" name="PiggyBanks[${index}].CurrentAmount" class="form-control" value="0" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Aportación mensual</label>
                    <input type="number" step="0.01" name="PiggyBanks[${index}].MonthlyContribution" class="form-control" value="0" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Icono (opcional)</label>
                    <input name="PiggyBanks[${index}].Icon" class="form-control" placeholder="Ej: ✈️" />
                </div>
            </div>
        `;
        
        container.appendChild(card);
    });

    container.addEventListener("click", function (e) {
        if (e.target.classList.contains("pb-delete-btn")) {
            const card = e.target.closest(".pb-card");
            card.remove();
        }
    });
});
