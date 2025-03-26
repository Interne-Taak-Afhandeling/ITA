<template>
    <div class="internetaken-component">
      <h1>Internetaken Dashboard</h1>
      <p>This component demonstrates fetching and displaying Internetaken data from the server.</p>
  
      <div v-if="loading" class="loading">
        <p>Loading... Please wait while we fetch the data.</p>
      </div>
  
      <div v-if="error" class="error">
        <p>{{ error }}</p>
        <p>For now, we're showing the WeatherForecast API data to demonstrate connectivity:</p>
      </div>
  
      <!-- Demo forecast data -->
      <div v-if="forecasts.length" class="content">
        <h3>Weather Forecast Data</h3>
        <table class="data-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Temp. (C)</th>
              <th>Temp. (F)</th>
              <th>Summary</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="forecast in forecasts" :key="forecast.date">
              <td>{{ formatDate(forecast.date) }}</td>
              <td>{{ forecast.temperatureC }}°C</td>
              <td>{{ forecast.temperatureF }}°F</td>
              <td>{{ forecast.summary }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: 'InternetakenList',
    data() {
      return {
        loading: true,
        error: null,
        forecasts: []
      }
    },
    async created() {
      await this.fetchData()
    },
    methods: {
      async fetchData() {
        this.loading = true
        this.error = null
        
        try {
          // Fetch weather forecast data (as a demo/placeholder)
          const response = await fetch('/weatherforecast')
          
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`)
          }
          
          this.forecasts = await response.json()
        } catch (e) {
          console.error('Error fetching data:', e)
          this.error = `Failed to load data: ${e.message}`
        } finally {
          this.loading = false
        }
      },
      formatDate(dateString) {
        // Convert the dateString to a readable format
        // The incoming format is like: 2023-03-27 from the API
        return new Date(dateString).toLocaleDateString()
      }
    }
  }
  </script>
  
  <style scoped>
  .internetaken-component {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
  }
  
  .loading {
    text-align: center;
    padding: 20px;
    color: #666;
  }
  
  .error {
    background-color: #ffebee;
    border-left: 4px solid #f44336;
    padding: 10px 15px;
    margin-bottom: 20px;
    border-radius: 4px;
  }
  
  .data-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 20px;
    box-shadow: 0 2px 3px rgba(0, 0, 0, 0.1);
  }
  
  .data-table th,
  .data-table td {
    padding: 12px 15px;
    text-align: left;
    border-bottom: 1px solid #ddd;
  }
  
  .data-table th {
    background-color: #f5f5f5;
    font-weight: bold;
  }
  
  .data-table tr:hover {
    background-color: #f9f9f9;
  }
  </style>