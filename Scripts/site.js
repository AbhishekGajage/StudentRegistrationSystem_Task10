/* Print the currently displayed student GridView cleanly. */
function printStudentGrid() {
    window.print();
}

/* Live preview of the uploaded profile photo before postback. */
function previewProfilePhoto(input, previewImgId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById(previewImgId).src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}
