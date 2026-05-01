// API Configuration
export const API_CONFIG = {
    BASE_URL: 'http://localhost:5127',
    ENDPOINTS: {
        EVENTS: '/api/events',
        SEATS: (eventId) => `/api/seats/event/${eventId}`,
        RESERVATIONS: '/api/reservations',
        SEATS_PURCHASE: '/api/seats/purchase',
        USERS: '/api/users',
        USER_BY_ID: (userId) => `/api/users/${userId}`
    }
};

// Storage Keys
export const STORAGE_KEYS = {
    USER: 'ticketapp_user',
    CURRENT_EVENT: 'ticketapp_current_event',
    SELECTED_SEAT: 'ticketapp_selected_seat'
};

// Pagination
export const PAGINATION = {
    DEFAULT_PAGE_SIZE: 4,
    DEFAULT_PAGE: 1
};

// Status Constants
export const SEAT_STATUS = {
    AVAILABLE: 'Available',
    RESERVED: 'Reserved',
    SOLD: 'Sold'
};

export const EVENT_STATUS = {
    ACTIVE: 'Active',
    CANCELLED: 'Cancelled',
    COMPLETED: 'Completed'
};

export const RESERVATION_STATUS = {
    PENDING: 'Pending',
    PAID: 'Paid',
    EXPIRED: 'Expired'
};

// Messages
export const MESSAGES = {
    LOGIN_SUCCESS: 'Ingreso exitoso',
    LOGIN_ERROR: 'Error al ingresar. Verifica tus credenciales.',
    RESERVATION_SUCCESS: 'Asiento reservado exitosamente',
    RESERVATION_ERROR: 'Error al reservar el asiento',
    SEAT_UNAVAILABLE: 'Este asiento no está disponible',
    NO_SEAT_SELECTED: 'Por favor selecciona un asiento',
    LOADING_EVENTS: 'Cargando eventos...',
    LOADING_SEATS: 'Cargando asientos...',
    NO_EVENTS: 'No hay eventos disponibles',
    NETWORK_ERROR: 'Error de conexión. Intenta más tarde.'
};

// Date/Time Formatting
export const DATE_FORMAT_OPTIONS = {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
};
