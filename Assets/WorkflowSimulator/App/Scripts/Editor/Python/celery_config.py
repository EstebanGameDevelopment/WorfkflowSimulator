from celery import Celery

# Configure Celery app
celery_app = Celery(
    "screen_manager",
    broker="redis://localhost:6379/0",  # Redis as broker
    backend="redis://localhost:6379/0",  # Store task results in Redis
)

# Celery configuration (optional)
celery_app.conf.update(
    task_serializer="json",
    accept_content=["json"],  # Ignore other content
    result_serializer="json",
    timezone="UTC",
    enable_utc=True,
)
