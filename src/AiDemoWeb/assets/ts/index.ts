import { ValidationService } from 'aspnet-client-validation';

const validationService = new ValidationService();
validationService.bootstrap({
    root: document.documentElement,
});
