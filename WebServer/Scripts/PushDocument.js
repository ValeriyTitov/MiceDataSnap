function HandleSuccess(text) {

    //document.getElementById("memoResult").innerHTML = "";
    document.getElementById("memoResult").innerHTML = text;
   
}

function HandleError(xhr, status, errorThrown) {
    //alert(errorThrown);
   document.getElementById("memoResult").value = errorThrown;
}

function InternalPush(DocumentsId, dfPathFoldersId, dfMethodsIdTarget, DbName)
{
 //   alert("Pushing to &DocumentsId=" + DocumentsId + "&dfPathFoldersId=" + dfPathFoldersId + "&dfMethodsIdTarget=" + dfMethodsIdTarget + "&DbName=" + DbName);
    $.ajax(
        {
            type: "POST",
            url: "/home/InternalPushDocumentWeb",
            data: "&DocumentsId=" + DocumentsId + "&dfPathFoldersId=" + dfPathFoldersId + "&dfMethodsIdTarget=" + dfMethodsIdTarget + "&DbName=" + DbName,
            success: HandleSuccess,
            error: HandleError
        }
    );
}

function TryPushDocument() {
        event.preventDefault()
        InternalPush(document.getElementById("edDocumentsId").value,
            document.getElementById("eddfPathFoldersId").value,
            document.getElementById("eddfMethodsIdTarget").value,
            document.getElementById("edDbName").value)
}