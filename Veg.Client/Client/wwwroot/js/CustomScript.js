const trumbowyg = window.trumbowyg;
var GLOBAL = {};
GLOBAL.DotNetReference = null;

GLOBAL.SetDotnetReference = function (pDotNetReference) {
        GLOBAL.DotNetReference = pDotNetReference;
};

function InitializeEditor(editorName) {

    $('#' + editorName).trumbowyg(
        {
            lang: 'nl',
            plugins: {
                table: {
                    // Some table plugin options, see details below
                    styler: ''
                },
                resizimg: {
                    minSize: 64,
                    step: 16,
                }
            },
            btns: [
                ['viewHTML'],
                ['undo', 'redo'], // Only supported in Blink browsers
                ['formatting'],
                ['fontfamily'],
                ['fontsize'],
                ['table'],
                ['strong', 'em', 'del'],
                ['superscript', 'subscript'],
                ['foreColor', 'backColor'],
                ['link'],
                ['insertImage', 'base64'],
                ['justifyLeft', 'justifyCenter', 'justifyRight', 'justifyFull'],
                ['unorderedList', 'orderedList'],
                ['horizontalRule'],
                ['removeformat'],
                ['fullscreen']
            ]
        });
}

function getBase64(file) {
    var reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = function () {
        console.log(reader.result);
    };
    reader.onerror = function (error) {
        console.log('Error: ', error);
    };
}

function GetTextFromEditor(editorName) {
    var html = $('#' + editorName).trumbowyg('html');
    return html;
}
function HookFileInputChangedListener(instance, inputName, functionName) {
    $('#' + inputName).change(function (e) {
        instance.invokeMethodAsync(functionName).then(r => console.log("attachment selected:" + r));
    });
}
function StopBarCode() {
    Quagga.stop();
}
async function GetBarCode() {
    const videoDevices = await Quagga.CameraAccess.enumerateVideoDevices();
    const videoDevice = videoDevices[videoDevices.length - 1];
    console.log(videoDevice);
    Quagga.init({
        inputStream: {
            name: "Live",
            type: "LiveStream",
            target: document.querySelector('#MediaStreamVideo')
            , constraints: {
                aspectRatio: { min: 1, max: 100 }, facingMode: "environment", deviceId: videoDevice.deviceId // or user 
            }
        },
        decoder: {
            readers: [
                "ean_reader"
            ],
            multiple: false
        }
    }, function (err) {
        if (err) {
            console.log(err);
            return
        }
        console.log("Initialization finished. Ready to start");
        Quagga.start();

        Quagga.onDetected(function (result) {
            var last_code = result.codeResult.code;
            console.log(last_code);
            Quagga.stop();
            GLOBAL.DotNetReference.invokeMethodAsync('RecognizedBarcode', last_code);
            return last_code;
        });
    });
}

function HookGroupInstance(inputName, instance) {
    $('#' + inputName).data('instance', instance);
}

function scrollToElementId(elementId){
    console.info('scrolling to element', elementId);
    var element = document.getElementById(elementId);
    if (!element) {
        console.warn('element was not found', elementId);
        return false;
    }
    element.scrollIntoView();
    return true;
}

function TriggerClick(inputName) {
    $('#' + inputName).trigger('click');
}

function TriggerClickOfElement(element) {
    $('#' + element).trigger('click');
}

function ChangeTitelOfPage(title) {
    window.document.title = title;
}

function InitializeDateTimePicker(editorName, startValue) {
    $('#' + editorName).datetimepicker({ footer: true, modal: true, format: 'dd-mm-yyyy HH:MM', value: startValue });
}

function InitializeDatePicker(editorName, startValue) {
    $('#' + editorName).datepicker({ footer: true, modal: true, format: 'dd-mm-yyyy', value: startValue });
}

function InitializeTimePicker(editorName) {
    $('#' + editorName).timepicker({ footer: true, modal: true, format: 'HH:MM' });
}

function GetValueTimePicker(editorName) {
    return $('#' + editorName).timepicker().value();
}

function GetValueDatePicker(editorName) {
    return $('#' + editorName).datepicker().value();
}

function GetValueDateTimePicker(editorName) {
    return $('#' + editorName).datetimepicker().value();
}

function WriteCookie(name, value, days) {

    var expires;
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    }
    else {
        expires = "";
    }
    document.cookie = name + "=" + value + expires + "; path=/";
}
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function RemoveCookie(name,) {
    document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT';
}

