# Deployment Documentation

Deployment/runbook and CI/CD pipeline guidance.

## Basic production deployment steps

1. Merge main branch
2. Run CI pipeline
3. Execute unit & integration tests
4. Build Docker images
5. Push images to registry
6. Deploy API and React apps
7. Apply database migrations
8. Verify health endpoints and monitoring

Include pipeline configuration, rollback steps, and environment specifics.
