
var recievedWordsList = [];

document.getElementById("textform").addEventListener("submit", async function (event) {
    try {
        event.preventDefault();
        // console.log(event.target);
        var { textForAnalyzing, topList, wordsToSplit } = event.target;

        if (textForAnalyzing?.value === undefined) {
            window.alert("Text to analyze not found");
            return 0;
        }
        if (topList?.value === undefined) {
            window.alert("Top not found");
            return 0;
        }
        if (wordsToSplit?.value === undefined) {
            wordsToSplit.value = 1;
        }
       
        const datasToSend = {
            text: textForAnalyzing.value,
            top: topList.value,
            wordsToSplit: wordsToSplit.value,
        }

        const response = await fetch("https://localhost:44301/api/initial/AnalyzeText", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(datasToSend)
        });


        if (response.ok === true) {
            const data = await response.json();
            recievedWordsList = data?.item3;
            document.getElementById("unique-words").innerText=`Unique phrase:${data?.item2}`
            document.getElementById("total-words").innerText=`Total phrase:${data?.item1}`
            document.getElementById("statistic-words").innerText=`Words in statistics:${data?.item3?.length}`

            var tbody = document.getElementById('words-table').getElementsByTagName('tbody')[0];
            tbody.innerHTML = '';
            let topList = document.getElementById("topList")?.value;
            if(topList===undefined || topList<=0 || topList>data?.item3?.length){
                topList = data?.item3?.length;
            }

            for (let i = 0; i < topList; i++) {
                var tr = document.createElement('tr');
                var td1 = document.createElement('td');
                var td2 = document.createElement('td');
                var td3 = document.createElement('td');
                td1.innerText = i+1;
                td2.innerText = data?.item3[i].item1;
                td3.innerText = data?.item3[i].item2 + " (" + data?.item3[i].item3 + "%)";
                tr.appendChild(td1);
                tr.appendChild(td2);
                tr.appendChild(td3);
                tbody.appendChild(tr);
            }
           
            // data?.item3.forEach(word => {
            //     var tr = document.createElement('tr');
            //     var td1 = document.createElement('td');
            //     var td2 = document.createElement('td');
            //     td1.innerText = word.item1;
            //     td2.innerText = word.item2 + " (" + word.item3 + "%)";
            //     tr.appendChild(td1);
            //     tr.appendChild(td2);
            //     tbody.appendChild(tr);

            // });

            console.log(data);
        }
        else {
            const data = await response.text();
            window.alert(data);
        }
    }
    catch (e) {
        window.alert(e);
    }
});


var topList = document.getElementById("topList");
['keyup', 'change'].forEach(evt =>
    topList.addEventListener(evt, (event) => {
        try {
            if (recievedWordsList.length <= 0) {
                return 0;
            }
            let top = event.target.value;
            // console.log(event.target)
            // console.log(top)
            if (top < 0 || top > recievedWordsList.length) {
                topList.value = recievedWordsList.length;
                top = recievedWordsList.length;
            }
            // if (top < 0 || top > recievedWordsList.length) {
            //     topList.value = recievedWordsList.length;
            //     top = recievedWordsList.length;
            // }

            var tbody = document.getElementById('words-table').getElementsByTagName('tbody')[0];
            tbody.innerHTML = '';

            for (let i = 0; i < top; i++) {
                var tr = document.createElement('tr');
                var td1 = document.createElement('td');
                var td2 = document.createElement('td');
                var td3 = document.createElement('td');
                td1.innerText = i+1;
                td2.innerText = recievedWordsList[i].item1;
                td3.innerText = recievedWordsList[i].item2 + " (" + recievedWordsList[i].item3 + "%)";
                tr.appendChild(td1);
                tr.appendChild(td2);
                tr.appendChild(td3);
                tbody.appendChild(tr);
            }
        }
        catch (e) {
            window.alert(e);
        }
    }));