import axios from 'axios';

export const api = axios.create({
    baseURL: 'http://localhost:3001', // Replace with your API root
    headers: {
        'Content-Type': 'application/json',
    },
});

export const localApi = axios.create({
    baseURL: '/api', // Replace with your API root
    headers: {
        'Content-Type': 'application/json',
    },
});

export const setAuthToken = (token: string) => {
    if (token) {
        api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
        delete api.defaults.headers.common['Authorization'];
    }
};