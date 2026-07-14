function setupAjaxFilter(formId, containerId) {

    $("#" + formId).submit(function (e) {

        e.preventDefault();

        var form = $(this);

        // Show Loading Spinner
        $("#" + containerId).html(
            `<div class="text-center p-5">
                <div class="spinner-border text-primary" role="status"></div>
                <h5 class="mt-3">Loading Report...</h5>
            </div>`
        );

        $.ajax({

            url: form.attr("action"),

            type: form.attr("method"),

            data: form.serialize(),

            success: function (result) {

                $("#" + containerId).html(result);

            },

            error: function () {

                $("#" + containerId).html(
                    `<div class="alert alert-danger text-center">
                        <h5>❌ Unable to load report.</h5>
                        <p>Please try again.</p>
                    </div>`
                );

            }

        });

    });

}