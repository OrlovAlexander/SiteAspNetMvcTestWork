function GetAntiForgeryToken() {
    var tokenField = $("input[type='hidden'][name$='RequestVerificationToken']");
    if (tokenField.length === 0) {
        return null;
    } else {
        return {
            name: tokenField[0].name,
            value: tokenField[0].value
        };
    }
}
 
$.ajaxPrefilter(
    function(options, localOptions, jqXHR) {
        if (options.type !== "GET") {
            var token = GetAntiForgeryToken();
            if (token !== null) {
                if (options.data.indexOf("X-Requested-With") === -1) {
                    options.data = "X-Requested-With=XMLHttpRequest" + (options.data === "" ? "" : "&" + options.data);
                 }
                 options.data = options.data + "&" + token.name + '=' + token.value;
             }
         }
     }
);

var deleteDocument = function (context) {
    var $ctrl = $(context);
    if (confirm('Действительно хотите удалить этот документ?')) {
        var newUrl = document.URL += "/Delete";
        $.ajax({
            url: newUrl,
            type: 'POST',
            data: { id: $(context).data('id') }
        }).done(function (data) {
            if (data.Result === "OK") {
                $ctrl.closest('tr').remove();
            }
            else if (data.Result !== undefined) {
                alert(data.Result.Message);
            }
        }).fail(function () {
            console.error("There is something wrong. Please try again.");
        });
    }
};

var informationTimer = function () {
    setTimeout(function tick() {
        if (document.getElementById("infoSection") !== undefined) {
            informationStore();
        }
        informationTimer = setTimeout(tick, 1000);
    }, 1000);
};

var informationStore = function () {
    var newUrl = document.URL += "/InformationStore";
    $.ajax({
        url: newUrl,
        type: 'POST',
        data: {}
    }).done(function (data) {
        if (data.Result === "OK") {
            document.getElementById("fileCount").innerText = data.Count;
            document.getElementById("fileVolume").innerText = data.Volume;
        }
        else if (data.Result !== undefined) {
            console.error(data.Result.Message);
        }
        }).fail(function () {
            console.error("There is something wrong. Please try again.");
    });
};

$(document).ready(function () {
    $('#filesTable').DataTable({
        "columnDefs": [
            { "targets": [3, 4], "orderable": false },
            { "targets": [3, 4], "searchable": false },
            { "targets": 1, "render": $.fn.dataTable.render.intlDateTime('ru-RU', { year: 'numeric', month: '2-digit', day: '2-digit' }) }
        ],
        "language": {
            "processing": "Подождите...",
            "search": "Поиск:",
            "lengthMenu": "Показать _MENU_ записей",
            "info": "Записи с _START_ до _END_ из _TOTAL_ записей",
            "infoEmpty": "Записи с 0 до 0 из 0 записей",
            "infoFiltered": "(отфильтровано из _MAX_ записей)",
            "infoPostFix": "",
            "loadingRecords": "Загрузка записей...",
            "zeroRecords": "Записи отсутствуют.",
            "emptyTable": "В таблице отсутствуют данные",
            "paginate": {
                "first": "Первая",
                "previous": "Предыдущая",
                "next": "Следующая",
                "last": "Последняя"
            },
            "aria": {
                "sortAscending": ": активировать для сортировки столбца по возрастанию",
                "sortDescending": ": активировать для сортировки столбца по убыванию"
            }
        }

    });
});
