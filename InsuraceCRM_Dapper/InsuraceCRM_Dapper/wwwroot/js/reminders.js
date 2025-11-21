(function ($) {
    const modalElement = document.getElementById('reminderModal');
    if (!modalElement) {
        return;
    }

    const modal = new bootstrap.Modal(modalElement);
    const customerNameEl = $('#reminderCustomerName');
    const reminderDateEl = $('#reminderDateTime');
    const reminderNoteEl = $('#reminderNote');
    const reminderIdInput = $('#reminderId');
    const callButton = $('#reminderCallButton');
    const doneButton = $('#reminderDoneButton');
    const antiForgeryToken = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();

    const fetchReminders = () => {
        $.get('/Reminder/GetDueReminders', (reminders) => {
            if (reminders && reminders.length > 0) {
                showReminder(reminders[0]);
            }
        });
    };

    const showReminder = (reminder) => {
        reminderIdInput.val(reminder.id);
        customerNameEl.text(reminder.customerName);
        reminderDateEl.text(new Date(reminder.reminderDateTime).toLocaleString());
        reminderNoteEl.text(reminder.note || 'No note provided');
        callButton.attr('href', reminder.customerMobileNumber ? `tel:${reminder.customerMobileNumber}` : '#');
        modal.show();
    };

    const markReminderDone = () => {
        const reminderId = reminderIdInput.val();
        if (!reminderId) {
            return;
        }

        $.ajax({
            url: '/Reminder/MarkAsShown',
            method: 'POST',
            data: { reminderId: reminderId },
            headers: {
                'RequestVerificationToken': antiForgeryToken
            }
        }).always(() => {
            modal.hide();
        });
    };

    doneButton.on('click', markReminderDone);

    fetchReminders();
    setInterval(fetchReminders, 60000);
})(jQuery);
