import { apiService } from '../api.js';
import { STORAGE_KEYS } from '../utils/constants.js';

export class LoginController {
    constructor(router) {
        this.router = router;
        this.modal = document.getElementById('loginModal');
        this.userSelect = document.getElementById('userSelect');
        this.passwordInput = document.getElementById('passwordInput');
        this.loginForm = document.getElementById('loginForm');
        this.loginError = document.getElementById('loginError');
        this.usersLoadingSpinner = document.getElementById('usersLoadingSpinner');
        
        this.initListeners();
        this.loadUsers();
    }

    initListeners() {
        this.loginForm.addEventListener('submit', (e) => this.handleLogin(e));
        
        // Cerrar modal al hacer clic fuera del contenido
        window.addEventListener('click', (e) => {
            if (e.target === this.modal) {
                this.hideModal();
            }
        });
    }

    /**
     * Load users from backend and populate dropdown
     */
    async loadUsers() {
        try {
            this.usersLoadingSpinner.style.display = 'block';
            this.userSelect.disabled = true;

            const users = await apiService.getUsers();
            this.userSelect.innerHTML = '<option value="">Selecciona un usuario...</option>';

            // Handle both array and paginated response formats
            const userList = Array.isArray(users) ? users : (users.data || users.users || []);

            userList.forEach(user => {
                const option = document.createElement('option');
                option.value = user.id;
                option.textContent = user.name || user.email;
                this.userSelect.appendChild(option);
            });

            this.usersLoadingSpinner.style.display = 'none';
            this.userSelect.disabled = false;
        } catch (error) {
            console.error('Error loading users:', error);
            this.usersLoadingSpinner.style.display = 'none';
            this.showError('Error cargando usuarios. Intenta nuevamente.');
            this.userSelect.disabled = false;
        }
    }
    
    /**
     * Handle login submission
     */
    async handleLogin(e) {
        e.preventDefault();
        
        const userId = this.userSelect.value;
        const password = this.passwordInput.value;

        if (!userId) {
            this.showError('Por favor selecciona un usuario');
            return;
        }

        try {
            // Get the selected user's details from the dropdown
            const selectedOption = this.userSelect.options[this.userSelect.selectedIndex];
            const userName = selectedOption.textContent;

            // Create user object with login info
            const user = {
                id: parseInt(userId),
                name: userName,
                password: password, // Store for reference (not validated against backend)
                token: 'session-token-' + Date.now() // Generate a session token
            };

            // Store in sessionStorage instead of localStorage
            sessionStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
            this.hideModal();
            
            console.log('✓ Usuario autenticado:', userName);
            
            // Redirect to events page
            this.router.navigate('events');
        } catch (error) {
            console.error('Error during login:', error);
            this.showError('Error durante el login. Intenta nuevamente.');
        }
    }

    /**
     * Show login modal
     */
    showModal() {
        this.modal.style.display = 'flex';
        this.loginForm.reset();
        this.loginError.style.display = 'none';
    }

    /**
     * Hide login modal
     */
    hideModal() {
        this.modal.style.display = 'none';
    }

    /**
     * Show error message
     */
    showError(message) {
        this.loginError.textContent = message;
        this.loginError.style.display = 'block';
    }
}
