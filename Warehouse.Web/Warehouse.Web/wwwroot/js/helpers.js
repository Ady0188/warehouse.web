window.downloadFile = function (url, filename) {
    const a = document.createElement('a');
    a.href = url;
    a.download = filename; // Specify the default name for the file
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

window.BlazorFocusHelpers = {
    focusAndSelect: function (id) {
        var element = document.getElementById(id);
        element.focus();

        if (id !== "myButton" && id !== "choseButton") {
            element.select();
        }
    },
    focusAndOpen: function (id) {
        var element = document.getElementById(id);
        element.focus();
        var event = new MouseEvent('click');
        element.dispatchEvent(event);
    },
    focusButton: function (id) {
        var element = document.getElementById(id);
        element.focus();
    }
};

window.printFile = (printContents) => {

    const parser = new DOMParser();
    const doc = parser.parseFromString(printContents, 'text/html');

    var printContent = doc.getElementById("content"),
        windowUrl = '',
        uniqueName = new Date(),
        windowName = 'Print' + uniqueName.getTime(),
        WinPrint;

    WinPrint = window.open(windowUrl, windowName, 'left=300,top=300,right=500,bottom=500,width=1000,height=500');
    WinPrint.document.write('<' + 'html' + '><head><style type="text/css">@page { size: auto;  margin: 1cm 1cm 1cm 2cm; } p { text-align:justify; }</style></head><' + 'body style="background:none !important;"' + '>');
    WinPrint.document.write(printContents);
    WinPrint.document.write('<' + '/body' + '><' + '/html' + '>');
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
};