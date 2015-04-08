(function () {
    'use strict';

    angular
        .module('app')
        .factory('utils', utils);

    function utils() {
        var service = {
            disableSubmitButton: disableSubmitButton,
            disableSubmitInput: disableSubmitInput,
            enableSubmitButton: enableSubmitButton,
            enableSubmitInput: enableSubmitInput,
            calculateAge: calculateAge,
            attachJQueryForPictureUploader: attachJQueryForPictureUploader
        };

        return service;

        function disableSubmitInput() {
            $(function () {
                $("input[type=submit]").attr("disabled", "disabled");
            });
        }

        function disableSubmitButton() {
            $(function () {
                $("button[type=submit]").attr("disabled", "disabled");
            });
        }

        // source: http://stackoverflow.com/questions/1414365/disable-enable-an-input-with-jquery
        function enableSubmitInput() {
            $(function () {
                $("input[type=submit]").removeAttr('disabled');
            });
        }

        // source: http://stackoverflow.com/questions/1414365/disable-enable-an-input-with-jquery
        function enableSubmitButton() {
            $(function () {
                $("button[type=submit]").removeAttr('disabled');
            });
        }

        // source: http://stackoverflow.com/questions/4060004/calculate-age-in-javascript
        function calculateAge(dateString) {
            var today = new Date();
            var birthDate = new Date(dateString);
            var age = today.getFullYear() - birthDate.getFullYear();
            var m = today.getMonth() - birthDate.getMonth();
            if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                age--;
            }
            return age;
        }

        function attachJQueryForPictureUploader(uploadUrl, idPicture, getImage) {
            // Attach jQuery
            $(document).ready(function () {

                $('#btnUploadImage').on('click', function () {

                    var files = $('#uploadImage').get(0).files;

                    // Add the uploaded image content to the form data collection

                    if (files.length > 0) {

                        var data = null;

                        if (typeof (FormData) == 'undefined') {
                            data = [];
                            data.push('UploadedImage', files[0]);
                        }
                        else {
                            data = new FormData();
                            data.append('UploadedImage', files[0]);
                        }

                        var ajaxRequest = $.ajax({
                            type: 'POST',
                            url: uploadUrl + idPicture,
                            contentType: false,
                            processData: false,
                            data: data,
                            success: function (data, text) {
                                // forcing ng src reload
                                getImage(true);
                            },
                            error: function (request, status, error) {
                                toastr.error('Respuesta del servidor: ' + error + ' ' + status + '.',
                                'Error al cargar imagen');
                            }
                        });

                        ajaxRequest.done(function (xhr, textStatus) {
                            if (textStatus == 'success') {
                                toastr.success('La imagen ha sido guardada en el servidor');
                            }
                            else {
                                toastr.error('Ha ocurrido un error al cargar la imagen');
                            }
                        });

                    }
                    else {
                        toastr.warning('Seleccione una imagen para cargar...');
                    }

                });
            });
        }
    }
})();