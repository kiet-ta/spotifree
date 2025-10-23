async function loadPage(pageName) {
    try {
        const url = `/pages/${pageName}.html`;

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Page not found: ${url}`);
        }

        const htmlContent = await response.text();

        document.getElementById('content-container').innerHTML = htmlContent;
    } catch (error) {
        console.error("Error loading page:", error);
        document.getElementById('content-container').innerHTML = "<h1>Error!</h1>";
    }
}

window.onload = () => {
    loadPage('home');
};