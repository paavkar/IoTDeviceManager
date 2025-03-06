# Use the official Nginx image as the base image
FROM nginx:stable-alpine

# Remove the default Nginx config file
RUN rm -f /etc/nginx/conf.d/default.conf

# Copy the custom Nginx configuration into the container
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]