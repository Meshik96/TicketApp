import { API_CONFIG, STORAGE_KEYS, PAGINATION, MESSAGES } from './utils/constants.js'; // Ajustar ruta según estructura
class ApiService {
    constructor() {
        this.baseUrl = API_CONFIG.BASE_URL;
        this.token = this.getToken();
    }

    /**
     * Get stored authentication token
     */
    getToken() {
        const user = JSON.parse(localStorage.getItem(STORAGE_KEYS.USER) || '{}');
        return user.token || null;
    }

    /**
     * Set authentication token
     */
    setToken(token) {
        const user = JSON.parse(localStorage.getItem(STORAGE_KEYS.USER) || '{}');
        user.token = token;
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
        this.token = token;
    }

    /**
     * Create request headers
     */
    getHeaders(includeAuth = false) {
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        
        if (includeAuth && this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }
        
        return headers;
    }

    /**
     * Handle response
     */
    async handleResponse(response) {
        if (!response.ok) {
            let errorMessage = MESSAGES.NETWORK_ERROR;
            
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorMessage;
            } catch (e) {
                errorMessage = `Error ${response.status}: ${response.statusText}`;
            }
            
            throw new Error(errorMessage);
        }
        
        return response.json();
    }

    /**
     * Simulated login - In real app, this would call backend auth endpoint
     * For demo: accepts any email/password combination
     */
    async login(email, password) {
        try {
            // Simulate API call with delay
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // In production, you would send:
            // const response = await fetch(`${this.baseUrl}/api/auth/login`, {
            //     method: 'POST',
            //     headers: this.getHeaders(),
            //     body: JSON.stringify({ email, password })
            // });
            // return this.handleResponse(response);
            
            // Demo: Accept any credentials
            if (!email || !password) {
                throw new Error('Email y contraseña son requeridos');
            }
            
            const user = {
                id: Math.floor(Math.random() * 10000),
                email: email,
                name: email.split('@')[0],
                token: 'demo-token-' + Date.now()
            };
            
            localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
            this.setToken(user.token);
            
            return { success: true, user };
        } catch (error) {
            throw error;
        }
    }

    /**
     * Get paginated events
     */
    async getEvents(page = 1, pageSize = PAGINATION.DEFAULT_PAGE_SIZE) {
        try {
            const url = `${this.baseUrl}${API_CONFIG.ENDPOINTS.EVENTS}?page=${page}&pageSize=${pageSize}`;
            
            console.log('📡 Fetching events from:', url);
            
            const response = await fetch(url, {
                method: 'GET',
                headers: this.getHeaders(),
                mode: 'cors'
            });
            
            console.log('✅ Event response status:', response.status);
            return this.handleResponse(response);
        } catch (error) {
            console.error('❌ Error fetching events:', error.message);
            console.error('   Full error:', error);
            throw error;
        }
    }

    /**
     * Get seats for a specific event
     */
    async getSeats(eventId) {
        try {
            const url = `${this.baseUrl}${API_CONFIG.ENDPOINTS.SEATS(eventId)}`;
            
            const response = await fetch(url, {
                method: 'GET',
                headers: this.getHeaders()
            });
            
            return this.handleResponse(response);
        } catch (error) {
            console.error('Error fetching seats:', error);
            throw error;
        }
    }

    /**
     * Reserve a seat
     */
    async reserveSeat(userId, seatId) {
        try {
            const url = `${this.baseUrl}${API_CONFIG.ENDPOINTS.RESERVE}`;
            
            const payload = {
                userId: parseInt(userId),
                seatId: seatId
            };
            
            const response = await fetch(url, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify(payload)
            });
            
            return this.handleResponse(response);
        } catch (error) {
            console.error('Error reserving seat:', error);
            throw error;
        }
    }

    /**
     * Logout
     */
    logout() {
        localStorage.removeItem(STORAGE_KEYS.USER);
        localStorage.removeItem(STORAGE_KEYS.CURRENT_EVENT);
        localStorage.removeItem(STORAGE_KEYS.SELECTED_SEAT);
        this.token = null;
    }

    /**
     * Check if user is logged in
     */
    isLoggedIn() {
        const user = localStorage.getItem(STORAGE_KEYS.USER);
        return !!user;
    }

    /**
     * Get current user
     */
    getCurrentUser() {
        const user = JSON.parse(localStorage.getItem(STORAGE_KEYS.USER) || 'null');
        return user;
    }
}

// Create global API service instance
export const apiService = new ApiService();
