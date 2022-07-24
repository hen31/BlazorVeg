const QuillBetterTable = window.QuillBetterTable;

Quill.register({
    'modules/better-table': QuillBetterTable
}, true)


function InitializeEditor(editorName) {
    var toolbarOptions = [
        ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
        ['blockquote', 'code-block'],
        ['table'],
        [{ 'header': 1 }, { 'header': 2 }],               // custom button values
        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
        [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
        [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
        [{ 'direction': 'rtl' }],                         // text direction

        [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
        ['link', 'image', 'video'],
        [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
        [{ 'font': [] }],
        [{ 'align': [] }],

        ['clean']                                         // remove formatting button
    ];
    console.log('inputFile', '#' + editorName);
    var container = $('#' + editorName).get(0);
    var editor = new Quill(container, {
        modules: {
            toolbar: toolbarOptions,
            table: false,
            'better-table': {
                operationMenu: {
                    items: {
                        unmergeCells: {
                            text: 'Another unmerge cells name'
                        }
                    }
                }
            }
            ,
            keyboard: {
                bindings: quillBetterTable.keyboardBindings
            }
        }, theme: 'snow'
    });
    $('#' + editorName).data('quill', editor);
}

function GetTextFromEditor(editorName) {
    console.log('GetTextFromEditor', '#' + editorName);
    var html = $('#' + editorName).data('quill').root.innerHTML;
    console.log(html);
    return html;
}