﻿function insertTag(o, t1, t2)
{
    var t1 = t1;
    var t2 = t2;
    var val = o.value;
    var startEnd = getSelection(o);
    o.value = val.slice(0, startEnd.start) + t1 + val.slice(startEnd.start, startEnd.end) + t2 + val.slice(startEnd.end);
    o.selectionStart = o.selectionEnd = startEnd.end + t1.length;
    o.focus();
}

function getSelection(inputBox) {
    if ("selectionStart" in inputBox) { //gecko  
        return {
            start: inputBox.selectionStart,
            end: inputBox.selectionEnd
        }
    }
    //and now, the blinkered IE way  
    var bookmark = document.selection.createRange().getBookmark()
    var selection = inputBox.createTextRange()

    selection.moveToBookmark(bookmark)

    var before = inputBox.createTextRange()
    before.collapse(true)
    before.setEndPoint("EndToStart", selection)

    var beforeLength = before.text.length
    var selLength = selection.text.length
    return {
        start: beforeLength,
        end: beforeLength + selLength
    }
}

function setSelection(inputBox, start, end) {
    if (start > end) {
        start = end
    }
    if ("selectionStart" in inputBox) { //gecko  
        inputBox.setSelectionRange(start, end);
        return true;
    }
    else {
        r = inputBox.createTextRange();
        r.collapse(true);
        r.moveStart('character', start);
        r.moveEnd('character', end - start);
        r.select();
        return true;
    }
}


