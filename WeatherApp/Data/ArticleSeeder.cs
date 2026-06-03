using WeatherApp.Models;

namespace WeatherApp.Data
{
    public static class ArticleSeeder
    {
        public static void SeedArticles(AppDbContext context)
        {
            // Force reseed by removing existing articles
            if (context.Articles.Any())
            {
                context.Articles.RemoveRange(context.Articles);
                context.SaveChanges();
            }

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Complete Guide to Weather Forecasting: How Accurate Are Weather Predictions?",
                    Slug = "weather-forecasting",
                    PublishDate = DateTime.UtcNow.AddDays(-5),
                    Content = @"
                        <h2>Understanding Weather Forecasting</h2>
                        <p>Weather forecasting is a complex science that combines data from multiple sources to predict atmospheric conditions. In this comprehensive guide, we'll explore how meteorologists make predictions and what factors influence forecast accuracy.</p>

                        <h3>The Science Behind Weather Prediction</h3>
                        <p>Modern weather forecasting relies on sophisticated computer models that process vast amounts of data from satellites, weather stations, and radar systems. These models simulate the atmosphere's behavior based on physical laws and historical patterns.</p>

                        <h4>Key Forecasting Methods:</h4>
                        <ul>
                            <li><strong>Numerical Weather Prediction (NWP):</strong> Uses mathematical equations to simulate atmospheric processes</li>
                            <li><strong>Statistical Methods:</strong> Analyzes historical weather patterns to predict future conditions</li>
                            <li><strong>Ensemble Forecasting:</strong> Runs multiple models to provide a range of possible outcomes</li>
                            <li><strong>Analog Methods:</strong> Compares current conditions to similar historical situations</li>
                        </ul>

                        <h3>Forecast Accuracy Rates</h3>
                        <p>Weather forecast accuracy varies depending on the time frame and location:</p>
                        <ul>
                            <li><strong>1-3 Days:</strong> 80-90% accuracy for temperature and precipitation</li>
                            <li><strong>4-7 Days:</strong> 60-75% accuracy</li>
                            <li><strong>8-14 Days:</strong> 40-50% accuracy</li>
                            <li><strong>Beyond 14 Days:</strong> Limited predictability</li>
                        </ul>

                        <h3>Factors Affecting Forecast Accuracy</h3>
                        <p>Several factors influence how accurate weather predictions can be:</p>
                        <ul>
                            <li>Data quality from weather stations and satellites</li>
                            <li>Model resolution and computational power</li>
                            <li>Local geography and terrain</li>
                            <li>Seasonal variations</li>
                            <li>Extreme weather events</li>
                        </ul>

                        <h3>How to Interpret Weather Forecasts</h3>
                        <p>Understanding forecast terminology helps you make better decisions:</p>
                        <ul>
                            <li><strong>Probability of Precipitation (PoP):</strong> Chance of measurable rain or snow</li>
                            <li><strong>Confidence Level:</strong> How certain meteorologists are about the forecast</li>
                            <li><strong>Wind Gusts:</strong> Maximum wind speeds expected</li>
                            <li><strong>Feels Like Temperature:</strong> Apparent temperature including wind chill or heat index</li>
                        </ul>

                        <h3>The Future of Weather Forecasting</h3>
                        <p>Advances in artificial intelligence and machine learning are improving forecast accuracy. New satellite technology and increased computing power allow meteorologists to process more data and create more detailed predictions.</p>

                        <h3>Conclusion</h3>
                        <p>While weather forecasting has become increasingly accurate, it remains an imperfect science. Always check multiple sources and understand that forecasts are probabilistic predictions, not guarantees. For the most reliable information, consult official weather services like the National Weather Service or your local meteorological authority.</p>
                    "
                },
                new Article
                {
                    Title = "Weather Safety Tips: How to Stay Safe in Extreme Weather Conditions",
                    Slug = "weather-safety-tips",
                    PublishDate = DateTime.UtcNow.AddDays(-4),
                    Content = @"
                        <h2>Essential Weather Safety Guide</h2>
                        <p>Extreme weather can pose serious risks to your health and safety. This guide provides practical tips for staying safe during various weather conditions.</p>

                        <h3>Heat Wave Safety</h3>
                        <p>During extreme heat, your body can overheat rapidly, leading to heat exhaustion or heat stroke.</p>
                        <ul>
                            <li>Stay hydrated - drink plenty of water throughout the day</li>
                            <li>Avoid outdoor activities during peak heat hours (10 AM - 4 PM)</li>
                            <li>Wear light-colored, loose-fitting clothing</li>
                            <li>Use sunscreen with SPF 30 or higher</li>
                            <li>Check on elderly neighbors and pets regularly</li>
                            <li>Never leave children or pets in parked cars</li>
                        </ul>

                        <h3>Cold Wave Safety</h3>
                        <p>Extreme cold can cause frostbite and hypothermia within minutes.</p>
                        <ul>
                            <li>Layer your clothing for maximum warmth</li>
                            <li>Wear a hat, gloves, and scarf to protect extremities</li>
                            <li>Stay dry to prevent heat loss</li>
                            <li>Keep your home heated safely</li>
                            <li>Check weather forecasts before traveling</li>
                            <li>Limit time spent outdoors in extreme cold</li>
                        </ul>

                        <h3>Thunderstorm Safety</h3>
                        <p>Thunderstorms can produce lightning, hail, and strong winds.</p>
                        <ul>
                            <li>When thunder roars, go indoors immediately</li>
                            <li>Avoid using electrical appliances during storms</li>
                            <li>Stay away from windows and doors</li>
                            <li>Don't use landline phones during thunderstorms</li>
                            <li>If caught outside, avoid tall trees and open fields</li>
                            <li>Wait 30 minutes after the last thunder before going outside</li>
                        </ul>

                        <h3>Flood Safety</h3>
                        <p>Floods are among the most dangerous weather phenomena.</p>
                        <ul>
                            <li>Never walk or drive through floodwaters</li>
                            <li>Just 6 inches of moving water can knock you down</li>
                            <li>1 foot of water can sweep away most vehicles</li>
                            <li>Move to higher ground immediately if flooding occurs</li>
                            <li>Have an emergency kit ready during flood season</li>
                            <li>Know evacuation routes in your area</li>
                        </ul>

                        <h3>Winter Storm Safety</h3>
                        <p>Winter storms can create hazardous travel conditions and power outages.</p>
                        <ul>
                            <li>Stock up on supplies before storms arrive</li>
                            <li>Keep your vehicle maintained and fueled</li>
                            <li>Have emergency supplies in your car</li>
                            <li>Avoid traveling during active snowfall</li>
                            <li>Clear snow and ice from windows and lights</li>
                            <li>Use sand or kitty litter for traction</li>
                        </ul>

                        <h3>Tornado Safety</h3>
                        <p>Tornadoes are violent and unpredictable weather phenomena.</p>
                        <ul>
                            <li>Have a safe room or shelter identified</li>
                            <li>Go to the lowest floor of a sturdy building</li>
                            <li>Stay away from windows</li>
                            <li>Listen to weather alerts on radio or phone</li>
                            <li>If outside, lie flat in a ditch or low-lying area</li>
                            <li>Never try to outrun a tornado in a vehicle</li>
                        </ul>

                        <h3>Creating an Emergency Kit</h3>
                        <p>Prepare for emergencies with these essential items:</p>
                        <ul>
                            <li>Water (1 gallon per person per day)</li>
                            <li>Non-perishable food</li>
                            <li>First aid kit</li>
                            <li>Flashlight and batteries</li>
                            <li>Battery-powered or hand-crank radio</li>
                            <li>Medications and medical equipment</li>
                            <li>Important documents in waterproof container</li>
                            <li>Cash and credit cards</li>
                        </ul>

                        <h3>Conclusion</h3>
                        <p>Weather safety is about being prepared and making smart decisions. Stay informed about weather conditions in your area, have a plan for emergencies, and don't hesitate to seek shelter when dangerous weather approaches. Your safety is the top priority.</p>
                    "
                },
                new Article
                {
                    Title = "Understanding Weather Patterns: A Beginner's Guide to Meteorology",
                    Slug = "weather-patterns",
                    PublishDate = DateTime.UtcNow.AddDays(-3),
                    Content = @"
                        <h2>Introduction to Weather Patterns</h2>
                        <p>Weather patterns are the result of complex interactions between the atmosphere, oceans, and land. Understanding these patterns helps us predict weather and appreciate the forces that shape our climate.</p>

                        <h3>Atmospheric Pressure Systems</h3>
                        <p>Pressure systems are the foundation of weather patterns.</p>
                        <ul>
                            <li><strong>High Pressure Systems:</strong> Associated with clear, stable weather</li>
                            <li><strong>Low Pressure Systems:</strong> Often bring clouds, precipitation, and storms</li>
                            <li><strong>Fronts:</strong> Boundaries between air masses with different temperatures</li>
                        </ul>

                        <h3>Wind Patterns</h3>
                        <p>Wind is created by pressure differences in the atmosphere.</p>
                        <ul>
                            <li><strong>Jet Streams:</strong> Fast-moving rivers of air in the upper atmosphere</li>
                            <li><strong>Trade Winds:</strong> Consistent winds near the equator</li>
                            <li><strong>Westerlies:</strong> Prevailing winds in mid-latitudes</li>
                            <li><strong>Local Winds:</strong> Created by local geography and temperature differences</li>
                        </ul>

                        <h3>Precipitation Patterns</h3>
                        <p>Precipitation occurs when water vapor condenses in the atmosphere.</p>
                        <ul>
                            <li><strong>Convective Precipitation:</strong> From rising air and thunderstorms</li>
                            <li><strong>Orographic Precipitation:</strong> From air forced up by mountains</li>
                            <li><strong>Frontal Precipitation:</strong> Along weather fronts</li>
                        </ul>

                        <h3>Seasonal Weather Patterns</h3>
                        <p>Weather patterns change with the seasons due to Earth's tilt and orbit.</p>
                        <ul>
                            <li><strong>Spring:</strong> Transitional season with variable weather</li>
                            <li><strong>Summer:</strong> Warm temperatures and afternoon thunderstorms</li>
                            <li><strong>Fall:</strong> Cooling temperatures and changing precipitation</li>
                            <li><strong>Winter:</strong> Cold temperatures and potential snow</li>
                        </ul>

                        <h3>Global Weather Patterns</h3>
                        <p>Large-scale patterns influence weather worldwide.</p>
                        <ul>
                            <li><strong>El Niño:</strong> Warm ocean temperatures affecting global weather</li>
                            <li><strong>La Niña:</strong> Cool ocean temperatures with opposite effects</li>
                            <li><strong>Monsoons:</strong> Seasonal wind reversals bringing heavy rain</li>
                            <li><strong>Hurricane Season:</strong> Peak activity in late summer and fall</li>
                        </ul>

                        <h3>Reading Weather Maps</h3>
                        <p>Weather maps show atmospheric conditions and help predict future weather.</p>
                        <ul>
                            <li>Isobars show pressure patterns</li>
                            <li>Fronts indicate boundaries between air masses</li>
                            <li>Colors often represent temperature or precipitation</li>
                            <li>Arrows show wind direction and speed</li>
                        </ul>

                        <h3>Conclusion</h3>
                        <p>Weather patterns are the result of fundamental atmospheric processes. By understanding these patterns, you can better interpret forecasts and appreciate the dynamic nature of our atmosphere.</p>
                    "
                },
                new Article
                {
                    Title = "The Impact of Climate Change on Weather: What You Need to Know",
                    Slug = "climate-change-weather",
                    PublishDate = DateTime.UtcNow.AddDays(-2),
                    Content = @"
                        <h2>Climate Change and Weather: Understanding the Connection</h2>
                        <p>Climate change is altering weather patterns worldwide. Understanding these changes helps us prepare for the future and take action to mitigate impacts.</p>

                        <h3>How Climate Change Affects Weather</h3>
                        <p>Climate change doesn't just make everything warmer - it fundamentally alters weather patterns.</p>
                        <ul>
                            <li>More frequent heat waves and record temperatures</li>
                            <li>Increased intensity of precipitation events</li>
                            <li>More severe droughts in some regions</li>
                            <li>Changes in hurricane intensity and patterns</li>
                            <li>Shifting seasonal patterns</li>
                        </ul>

                        <h3>Rising Global Temperatures</h3>
                        <p>Average global temperatures have risen approximately 1.1°C since pre-industrial times.</p>
                        <ul>
                            <li>Arctic temperatures rising twice as fast as global average</li>
                            <li>Ocean temperatures increasing</li>
                            <li>Melting ice sheets and glaciers</li>
                            <li>Rising sea levels</li>
                        </ul>

                        <h3>Extreme Weather Events</h3>
                        <p>Climate change is increasing the frequency and intensity of extreme weather.</p>
                        <ul>
                            <li><strong>Heat Waves:</strong> More frequent and longer-lasting</li>
                            <li><strong>Heavy Rainfall:</strong> More intense precipitation events</li>
                            <li><strong>Droughts:</strong> Longer and more severe</li>
                            <li><strong>Wildfires:</strong> Increased frequency and intensity</li>
                            <li><strong>Hurricanes:</strong> Potentially stronger and slower-moving</li>
                        </ul>

                        <h3>Regional Impacts</h3>
                        <p>Climate change affects different regions in different ways.</p>
                        <ul>
                            <li><strong>Tropics:</strong> Increased rainfall variability</li>
                            <li><strong>Mid-latitudes:</strong> Shifting storm tracks</li>
                            <li><strong>Polar Regions:</strong> Rapid warming and ice loss</li>
                            <li><strong>Coastal Areas:</strong> Rising seas and increased flooding</li>
                        </ul>

                        <h3>What We Can Do</h3>
                        <p>Individual and collective action can help mitigate climate change.</p>
                        <ul>
                            <li>Reduce energy consumption</li>
                            <li>Use renewable energy sources</li>
                            <li>Support sustainable practices</li>
                            <li>Advocate for climate policies</li>
                            <li>Prepare for extreme weather</li>
                        </ul>

                        <h3>Conclusion</h3>
                        <p>Climate change is reshaping our weather patterns and will continue to do so for decades. By understanding these changes and taking action, we can work toward a more sustainable future.</p>
                    "
                },
                new Article
                {
                    Title = "Best Practices for Using Weather Apps: Maximize Your Weather App Experience",
                    Slug = "weather-app-guide",
                    PublishDate = DateTime.UtcNow.AddDays(-1),
                    Content = @"
                        <h2>Getting the Most Out of Your Weather App</h2>
                        <p>Weather apps are powerful tools for staying informed about atmospheric conditions. This guide helps you use them effectively.</p>

                        <h3>Choosing the Right Weather App</h3>
                        <p>Different apps offer different features and accuracy levels.</p>
                        <ul>
                            <li>Check data sources (government agencies vs. private companies)</li>
                            <li>Look for detailed forecasts and radar</li>
                            <li>Consider user interface and ease of use</li>
                            <li>Check for real-time alerts and notifications</li>
                            <li>Verify accuracy in your specific location</li>
                        </ul>

                        <h3>Understanding Weather App Features</h3>
                        <p>Most weather apps include these key features:</p>
                        <ul>
                            <li><strong>Current Conditions:</strong> Temperature, humidity, wind, and precipitation</li>
                            <li><strong>Hourly Forecast:</strong> Hour-by-hour predictions</li>
                            <li><strong>Extended Forecast:</strong> 7-14 day predictions</li>
                            <li><strong>Radar:</strong> Real-time precipitation visualization</li>
                            <li><strong>Alerts:</strong> Warnings for severe weather</li>
                            <li><strong>UV Index:</strong> Sun exposure risk</li>
                            <li><strong>Air Quality:</strong> Pollution levels</li>
                        </ul>

                        <h3>Interpreting Forecast Data</h3>
                        <p>Understanding forecast terminology helps you make better decisions.</p>
                        <ul>
                            <li><strong>Temperature:</strong> High and low for the day</li>
                            <li><strong>Feels Like:</strong> Apparent temperature including wind chill</li>
                            <li><strong>Precipitation Chance:</strong> Probability of rain or snow</li>
                            <li><strong>Wind Speed:</strong> Average and gust speeds</li>
                            <li><strong>Humidity:</strong> Moisture content in the air</li>
                        </ul>

                        <h3>Using Alerts Effectively</h3>
                        <p>Weather alerts can help you prepare for dangerous conditions.</p>
                        <ul>
                            <li>Enable notifications for your location</li>
                            <li>Customize alert types (severe weather, heat, cold, etc.)</li>
                            <li>Act quickly when alerts are issued</li>
                            <li>Have a plan for different types of alerts</li>
                        </ul>

                        <h3>Planning Activities with Weather Apps</h3>
                        <p>Use weather apps to plan outdoor activities safely.</p>
                        <ul>
                            <li>Check hourly forecasts for the best time to go outside</li>
                            <li>Plan around precipitation and temperature</li>
                            <li>Consider wind conditions for water activities</li>
                            <li>Check UV index for sun protection needs</li>
                            <li>Monitor forecasts as your activity date approaches</li>
                        </ul>

                        <h3>Tips for Accurate Forecasting</h3>
                        <ul>
                            <li>Check multiple sources for comparison</li>
                            <li>Pay attention to confidence levels</li>
                            <li>Remember forecasts are probabilistic, not certain</li>
                            <li>Short-term forecasts are more accurate than long-term</li>
                            <li>Local geography affects weather significantly</li>
                        </ul>

                        <h3>Conclusion</h3>
                        <p>Weather apps are valuable tools for staying informed and safe. By understanding their features and interpreting data correctly, you can make better decisions about your daily activities and long-term planning.</p>
                    "
                }
            };

            context.Articles.AddRange(articles);
            context.SaveChanges();
        }
    }
}
