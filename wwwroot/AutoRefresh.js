(function () {
    window.AutoRefresh = {
        refreshMTconnectData: () => {
            setInterval(() => {
                document.getElementById("TableRefreshBUT").click();
            }, 1000);
            setInterval(() => {
                document.getElementById("TableRefreshMainPage").click();
            }, 50000);
            
        }
    };
})();