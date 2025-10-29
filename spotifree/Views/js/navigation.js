async function loadPage(pageName) {
    try {
        const url = `/pages/${pageName}.html`;

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Page not found: ${url}`);
        }

        const htmlContent = await response.text();

        document.getElementById('content-container').innerHTML = htmlContent;
        if (pageName === 'library') {
            if (typeof initLibrary === 'function') {
                initLibrary();
            }else {
                console.error("initLibrary function not found");
            }
        } else if (pageName === 'home') {
            if (typeof initHome === 'function') {
                initHome();
            } else {
                console.error("initHome function not found");
            }
        }
    } catch (error) {
        console.error("Error loading page:", error);
        document.getElementById('content-container').innerHTML = "<h1>Error!</h1>";
    }
}

window.onload = () => {
    loadPage('home');
};