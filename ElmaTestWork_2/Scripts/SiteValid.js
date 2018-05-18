
// Валидация формы на клиенте
// ~/Scripts/SiteValid.js

(function () {
    document.forms.noValidate = true;
    $('form').on('submit', function (e) {
        var elements = this.elements;
        var valid = {};
        var isValid;
        var isFormValid;

        for (var i = 0, l = (elements.length - 1); i < l; i++) {
            isValid = validateRequired(elements[i]) && validateTypes(elements[i]);
            if (!isValid) {
                showErrorMessage(elements[i]);
            } else {
                removeErrorMessage(elements[i]);
            }
            valid[elements[i].id] = isValid;
        }

        // ИНДИВИДУАЛЬНАЯ ВАЛИДАЦИЯ
        if (!validateDescription()) {
            showErrorMessage(document.getElementById('Description'));
            valid.Description = false;
        } else {
            removeErrorMessage(document.getElementById('Description'));
        }

        //if (!validateDate()) {
        //    showErrorMessage(document.getElementById('Date'));
        //    valid.Date = false;
        //} else {
        //    removeErrorMessage(document.getElementById('Date'));
        //}

        if (!validateFiles()) {
            showErrorMessage(document.getElementById('Files'));
            valid.Files = false;
        } else {
            removeErrorMessage(document.getElementById('Files'));
        }

        // ОПРЕДЕЛЕНИЕ РЕЗУЛЬТАТА ПРОВЕРКИ
        isFormValid = true;
        for (var field in valid) {
            if (!valid[field]) {
                isFormValid = false;
                break;
            }
        }

        // ФОРМА НЕ ПРОШЛА ПРОВЕРКУ
        if (!isFormValid) {
            e.preventDefault();
        }
    });

    function validateRequired(el) {
        if (isRequired(el)) {
            var valid = !isEmpty(el);
            if (!valid) {
                setErrorMessage(el, 'Поле необходимо заполнить');
            }
            return valid;
        }
        return true;
    };

    function isRequired(el) {
        return ((typeof el.required === 'boolean') && el.required) ||
            (typeof el.required === 'string');
    };

    function isEmpty(el) {
        return !el.value || el.value === el.placeholder;
    };

    function setErrorMessage(el, message) {
        $(el).data('errorMessage', message);
    };

    function showErrorMessage(el) {
        var $el = $(el);
        var $errorContainer = $el.siblings('.error');
        if (!$errorContainer.length) {
            $errorContainer = $('<span class="error text-danger"></span>').insertAfter($el);
        }
        $errorContainer.text($(el).data('errorMessage'));
    };

    function removeErrorMessage(el) {
    };

    function validateTypes(el) {
        if (!el.value) return true;

        var type = $(el).data('type') || el.getAttribute('type');
        if (typeof validateType[type] === 'function') {
            return validateType[type](el);
        } else {
            return true;
        }
    };

    var validateType = {
        date: function (el) {
            var valid = /^(\d{2}\.\d{2}.\d{4})$/.test(el.value);
            if (!valid) {
                setErrorMessage(el, 'Проверить правильность даты');
            }
            return valid;
        }
    };

    function validateDescription() {
        var description = document.getElementById('Description');
        var valid = description.value.length > 0 && description.value.length <= 300;
        if (description.value.length === 0) {
            setErrorMessage(description, 'Заполните поле Описание документа.');
        }
        if (description.value.length > 300) {
            setErrorMessage(description, 'Превышен размер поля в 300 символов.');
        }
        return valid;
    };

    //function validateDate() {
    //    var date = document.getElementById('Date');
    //}

    function validateFiles() {
        var files = document.getElementById('Files');
        var valid = files.value.length > 0;
        if (!valid) {
            setErrorMessage(files, 'Выберите файлы.')
        }
        return valid;
    };

}());