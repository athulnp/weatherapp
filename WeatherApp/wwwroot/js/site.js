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

// Current Location Weather Functionality
document.addEventListener("DOMContentLoaded", function () {
    const currentLocationBtn = document.getElementById("get-current-location");
    const currentLocationSection = document.getElementById("current-location-weather");
    const locationError = document.getElementById("location-error");
    
    if (currentLocationBtn) {
        currentLocationBtn.addEventListener("click", function() {
            getCurrentLocationWeather();
        });
    }
    
    // Removed auto-fetch - now requires user to click the button
});

function getCurrentLocationWeather() {
    const currentLocationSection = document.getElementById("current-location-weather");
    const locationError = document.getElementById("location-error");
    const loadingSpinner = document.getElementById("location-loading");
    
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
    
    // Check cache first
    const cacheKey = `weather_${lat}_${lon}`;
    const cachedData = localStorage.getItem(cacheKey);
    const cacheTime = localStorage.getItem(`${cacheKey}_time`);
    
    // Cache for 20 minutes (1200000 ms) - current weather can change relatively quickly
    const CACHE_DURATION = 20 * 60 * 1000;
    
    if (cachedData && cacheTime) {
        const cacheAge = Date.now() - parseInt(cacheTime);
        if (cacheAge < CACHE_DURATION) {
            console.log("Using cached weather data");
            displayCurrentLocationWeather(JSON.parse(cachedData));
            if (loadingSpinner) {
                loadingSpinner.style.display = "none";
            }
            return;
        }
    }
    
    // Fetch from API if not cached or cache expired
    fetch(`/Home/GetWeatherByCoordinates?lat=${lat}&lon=${lon}`)
        .then(response => response.json())
        .then(data => {
            console.log("Backend response:", data);
            if (data.success && data.data) {
                console.log("Weather data:", data.data);
                // Cache the response
                localStorage.setItem(cacheKey, JSON.stringify(data.data));
                localStorage.setItem(`${cacheKey}_time`, Date.now().toString());
                
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
    
    // Hide empty state
    if (emptyState) {
        emptyState.style.display = "none";
    }
    
    const html = `
        <div class="weather-card-content">
            <div class="weather-card-left">
                <div class="weather-main-info">
                    <h3 class="weather-location">${weather.cityName || "Current Location"}</h3>
                    <p class="weather-description">${weather.description}</p>
                </div>
                
                <div class="weather-temperature">
                    <span class="temperature-value">${Math.round(weather.temperature)}</span>
                    <span class="temperature-unit">°C</span>
                </div>
            </div>
            
            <div class="weather-card-right">
                <img src="${weather.icon}" alt="${weather.description}" class="weather-icon" />
            </div>
        </div>
        
        <div class="weather-details-grid">
            <div class="weather-detail-item">
                <span class="detail-icon">🌡️</span>
                <div class="detail-info">
                    <span class="detail-label">Feels Like</span>
                    <span class="detail-value">${Math.round(weather.feelsLike)}°C</span>
                </div>
            </div>
            <div class="weather-detail-item">
                <span class="detail-icon">⬆️</span>
                <div class="detail-info">
                    <span class="detail-label">High</span>
                    <span class="detail-value">${Math.round(weather.tempMax)}°C</span>
                </div>
            </div>
            <div class="weather-detail-item">
                <span class="detail-icon">⬇️</span>
                <div class="detail-info">
                    <span class="detail-label">Low</span>
                    <span class="detail-value">${Math.round(weather.tempMin)}°C</span>
                </div>
            </div>
            <div class="weather-detail-item">
                <span class="detail-icon">💧</span>
                <div class="detail-info">
                    <span class="detail-label">Humidity</span>
                    <span class="detail-value">${weather.humidity}%</span>
                </div>
            </div>
            <div class="weather-detail-item">
                <span class="detail-icon">💨</span>
                <div class="detail-info">
                    <span class="detail-label">Wind</span>
                    <span class="detail-value">${weather.windSpeed} km/h</span>
                </div>
            </div>
        </div>
        
        <p class="cache-info" style="text-align: center; padding: 16px 32px; color: #888; font-size: 0.85rem; margin: 0;">
            🕐 Last updated: ${new Date().toLocaleTimeString()}
        </p>
    `;
    
    currentLocationSection.innerHTML = html;
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
