"use strict";

var face = {
    start: function () {
        $('#detect').on('click', face.recognize);
        $('#url').on('click', () => $('#errormessage').addClass('d-none'));
        $("#url").keyup(e => { if (e.keyCode === 13) $("#detect").click(); });
    },

    recognize: async function (e) {
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }
        $('#errormessage').addClass('d-none')
        $('#result').addClass('d-none');
        $('#detect').prop('disabled', true);

        let imageUrl = $('#url').val();
        try {
            let result = await $.get('Detect', { imageUrl });
            face.showResult(result, imageUrl);
        }
        catch(err)
        {
            $('#errormessage').removeClass('d-none').text(err.responseText);
        }
        
        $('#detect').prop('disabled', false);
        $('#url').val("");
    },

    showResult: function (result, url) {
        $('#resultImage').html('<img src="' + url + '"class="mw-100">');
        $("#result table tbody").html('');

        let tbody = $("#result table tbody")[0];
        if (result && result.length > 0) {
            for (var i = 0; i < result.length; i++) {
                var row = tbody.insertRow(tbody.rows.length);
                var cellName = row.insertCell(0);
                var cellDescription = row.insertCell(1); 
                var cellConfidence = row.insertCell(2); 
                cellName.innerHTML = result[i].name.trim();
                cellDescription.innerHTML = result[i].description.trim();
                cellConfidence.innerHTML = (result[i].confidence * 100).toFixed(2) + '%';
            }
        }
        $('#result').removeClass('d-none');
    }
};

face.start();