// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Smooth scroll to weather result after page load
window.addEventListener("load", function () {
    const result = document.getElementById("weather-result");
    if (result) {
        result.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
    }
});

// Add loading state to weather form
document.addEventListener("DOMContentLoaded", function () {
    const weatherForm = document.querySelector(".weather-search-form");
    if (weatherForm) {
        weatherForm.addEventListener("submit", function () {
            const weatherCard = document.querySelector(".weather-card");
            if (weatherCard) {
                weatherCard.classList.add("loading");
            }
        });
    }
});

// Cookie Consent Banner Functionality
document.addEventListener("DOMContentLoaded", function () {
    const cookieConsent = document.getElementById("cookie-consent");
    const acceptButton = document.getElementById("accept-cookies");
    const declineButton = document.getElementById("decline-cookies");
    
    // Check if user has already made a choice
    if (!localStorage.getItem("cookieConsent")) {
        // Show banner after a short delay
        setTimeout(function() {
            if (cookieConsent) {
                cookieConsent.style.display = "block";
            }
        }, 1000);
    } else {
        // Hide banner if choice already made
        if (cookieConsent) {
            cookieConsent.style.display = "none";
        }
    }
    
    // Handle accept button
    if (acceptButton) {
        acceptButton.addEventListener("click", function() {
            localStorage.setItem("cookieConsent", "accepted");
            if (cookieConsent) {
                cookieConsent.style.display = "none";
            }
            // Here you can enable Google Analytics or other tracking
            console.log("Cookies accepted");
        });
    }
    
    // Handle decline button
    if (declineButton) {
        declineButton.addEventListener("click", function() {
            localStorage.setItem("cookieConsent", "declined");
            if (cookieConsent) {
                cookieConsent.style.display = "none";
            }
            // Here you can disable Google Analytics or other tracking
            console.log("Cookies declined");
        });
    }
});

// Store current location weather in session memory
let currentWeatherData = null;

// Function to restore weather data from session memory on page load
function restoreWeatherDataFromSession() {
    const currentLocationSection = document.getElementById("current-location-weather");
    
    if (!currentLocationSection || !currentWeatherData) return;
    
    console.log("Restoring weather data from session memory");
    displayCurrentLocationWeather(currentWeatherData);
}

// Current Location Weather Functionality
document.addEventListener("DOMContentLoaded", function () {
    const currentLocationBtn = document.getElementById("get-current-location");
    const currentLocationSection = document.getElementById("current-location-weather");
    const locationError = document.getElementById("location-error");
    
    // Restore weather data from session memory if available
    restoreWeatherDataFromSession();
    
    if (currentLocationBtn) {
        currentLocationBtn.addEventListener("click", function() {
            getCurrentLocationWeather();
        });
    }
    
    // Vertical Tabs Functionality
    const verticalTabBtns = document.querySelectorAll('.vertical-tab-btn');
    const verticalTabPanes = document.querySelectorAll('.vertical-tab-pane');
    
    verticalTabBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            // Remove active class from all buttons and panes
            verticalTabBtns.forEach(b => b.classList.remove('active'));
            verticalTabPanes.forEach(p => p.classList.remove('active'));
            
            // Add active class to clicked button
            this.classList.add('active');
            
            // Show corresponding tab pane
            const tabId = this.getAttribute('data-tab');
            document.getElementById(tabId).classList.add('active');
        });
    });
    
    // Removed auto-fetch - now requires user to click the button
});

function getCurrentLocationWeather() {
    const currentLocationSection = document.getElementById("current-location-weather");
    const locationError = document.getElementById("location-error");
    const loadingSpinner = document.getElementById("location-loading");
    const emptyState = document.getElementById("current-location-empty");
    
    // Hide empty state immediately
    if (emptyState) {
        emptyState.classList.add("hidden");
        emptyState.style.display = "none";
    }
    
    // Show loading state
    if (loadingSpinner) {
        loadingSpinner.style.display = "block";
    }
    if (locationError) {
        locationError.style.display = "none";
    }
    
    if (!navigator.geolocation) {
        showLocationError("Geolocation is not supported by your browser");
        return;
    }
    
    navigator.geolocation.getCurrentPosition(
        function(position) {
            const lat = position.coords.latitude;
            const lon = position.coords.longitude;
            
            // Fetch weather data using coordinates
            fetchWeatherByCoordinates(lat, lon);
        },
        function(error) {
            let errorMessage = "Unable to retrieve your location";
            switch(error.code) {
                case error.PERMISSION_DENIED:
                    errorMessage = "Location permission denied. Please enable location access.";
                    break;
                case error.POSITION_UNAVAILABLE:
                    errorMessage = "Location information is unavailable.";
                    break;
                case error.TIMEOUT:
                    errorMessage = "Location request timed out.";
                    break;
            }
            showLocationError(errorMessage);
        },
        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 600000 // 10 minutes cache for location
        }
    );
}

function fetchWeatherByCoordinates(lat, lon) {
    const loadingSpinner = document.getElementById("location-loading");
    
    // Fetch from API
    fetch(`/Home/GetWeatherByCoordinates?lat=${lat}&lon=${lon}`)
        .then(response => response.json())
        .then(data => {
            console.log("Backend response:", data);
            if (data.success && data.data) {
                console.log("Weather data:", data.data);
                // Store in session memory
                currentWeatherData = data.data;
                displayCurrentLocationWeather(data.data);
            } else {
                console.log("Backend error:", data.error);
                showLocationError(data.error || "Failed to fetch weather data");
            }
        })
        .catch(error => {
            console.error("Error fetching weather:", error);
            showLocationError("Failed to fetch weather data. Please try again.");
        })
        .finally(() => {
            if (loadingSpinner) {
                loadingSpinner.style.display = "none";
            }
        });
}

function displayCurrentLocationWeather(weather) {
    const currentLocationSection = document.getElementById("current-location-weather");
    const emptyState = document.getElementById("current-location-empty");
    
    if (!currentLocationSection || !weather) return;
    
    // Hide empty state completely
    if (emptyState) {
        emptyState.classList.add("hidden");
        emptyState.style.display = "none";
    }
    
    const cityName = weather.cityName || "Current Location";
    const urlFriendlyName = cityName.toLowerCase().replace(" ", "-");
    
    const html = `
        <div class="weather-card-link">
            <div class="weather-header">
                <div class="weather-main-info">
                    <div class="weather-temp">
                        <span class="temperature-value">${Math.round(weather.temperature)}</span>
                        <span class="temperature-unit">°C</span>
                    </div>
                    <div class="weather-meta">
                        <p class="weather-description">${weather.description}</p>
                        <p class="weather-location">${cityName}</p>
                    </div>
                </div>
                <img src="${weather.icon}" alt="${weather.description}" class="weather-icon-compact" loading="lazy" width="48" height="48" />
            </div>
            <div class="weather-stats">
                <div class="stat-item">
                    <span class="stat-icon">🌡️</span>
                    <div class="stat-info">
                        <span class="stat-label">Feels Like</span>
                        <span class="stat-value">${Math.round(weather.feelsLike)}°C</span>
                    </div>
                </div>
                <div class="stat-item">
                    <span class="stat-icon">💧</span>
                    <div class="stat-info">
                        <span class="stat-label">Humidity</span>
                        <span class="stat-value">${weather.humidity}%</span>
                    </div>
                </div>
                <div class="stat-item">
                    <span class="stat-icon">💨</span>
                    <div class="stat-info">
                        <span class="stat-label">Wind</span>
                        <span class="stat-value">${weather.windSpeed} km/h</span>
                    </div>
                </div>
            </div>
            <a href="/weather/${urlFriendlyName}" class="weather-action-btn">
                <span>View Full Report</span>
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M5 12h14"></path>
                    <path d="M12 5l7 7-7 7"></path>
                </svg>
            </a>
        </div>
    `;
    
    currentLocationSection.innerHTML = html;
    currentLocationSection.classList.add("show");
    currentLocationSection.style.display = "block";
}

function showLocationError(message) {
    const locationError = document.getElementById("location-error");
    const loadingSpinner = document.getElementById("location-loading");
    const emptyState = document.getElementById("current-location-empty");
    
    if (loadingSpinner) {
        loadingSpinner.style.display = "none";
    }
    
    // Hide empty state on error
    if (emptyState) {
        emptyState.style.display = "none";
    }
    
    if (locationError) {
        locationError.innerHTML = `
            <div class="error-message">
                <p>❌ ${message}</p>
                <button onclick="getCurrentLocationWeather()" class="retry-btn">Retry</button>
            </div>
        `;
        locationError.style.display = "block";
    }
}
