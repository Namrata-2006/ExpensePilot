(() => {
  const modal = document.getElementById('expenseModal');
  const openButtons = [document.getElementById('openExpense'), document.getElementById('openExpenseEmpty')].filter(Boolean);
  const closeButtons = [document.getElementById('closeExpense'), document.getElementById('cancelExpense')].filter(Boolean);
  const form = document.getElementById('expenseForm');
  const typeInputs = document.querySelectorAll('input[name="Type"]');
  const spendingField = document.getElementById('spendingKindField');
  const monthPicker = document.getElementById('monthPicker');

  function openModal() {
    if (!modal) return;
    modal.classList.add('is-open');
    modal.setAttribute('aria-hidden', 'false');
    document.body.classList.add('modal-open');
    window.setTimeout(() => document.getElementById('Title')?.focus(), 60);
  }
  function closeModal() {
    if (!modal) return;
    modal.classList.remove('is-open');
    modal.setAttribute('aria-hidden', 'true');
    document.body.classList.remove('modal-open');
  }
  openButtons.forEach(button => button.addEventListener('click', openModal));
  closeButtons.forEach(button => button.addEventListener('click', closeModal));
  modal?.addEventListener('click', event => { if (event.target === modal) closeModal(); });
  document.addEventListener('keydown', event => { if (event.key === 'Escape') closeModal(); });
  function syncType() {
    const selected = document.querySelector('input[name="Type"]:checked')?.value;
    if (spendingField) {
      spendingField.style.opacity = selected === 'Income' ? '.45' : '1';
      const select = spendingField.querySelector('select');
      if (select) select.disabled = selected === 'Income';
    }
  }
  typeInputs.forEach(input => input.addEventListener('change', syncType));
  syncType();
  monthPicker?.addEventListener('change', () => {
    if (!monthPicker.value) return;
    const [year, month] = monthPicker.value.split('-');
    const url = new URL(window.location.href);
    url.searchParams.set('year', year);
    url.searchParams.set('month', month);
    window.location.href = url.toString();
  });
  form?.addEventListener('submit', event => {
    const amount = Number(document.getElementById('Amount')?.value);
    const date = document.getElementById('Date')?.value;
    if (!Number.isFinite(amount) || amount <= 0) {
      event.preventDefault();
      alert('Enter an amount greater than zero.');
    } else if (date && new Date(date + 'T00:00:00') > new Date(new Date().toDateString())) {
      event.preventDefault();
      alert('Choose today or an earlier date.');
    }
  });
})();
