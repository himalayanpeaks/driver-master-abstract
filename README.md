# OneDriver.Master.Abstract

**OneDriver.Master.Abstract** is a foundational .NET library that defines a common abstraction layer for communication masters across various industrial protocols. This includes, but is not limited to, IO-Link, Modbus, and CANopen.

It serves as the base interface and class definition that all protocol-specific master drivers should implement, ensuring a consistent and modular approach to device communication in the OneDriver ecosystem.

---

## ✨ Purpose

To provide a protocol-agnostic abstraction of master-side communication, enabling seamless integration and extension of new fieldbus or industrial communication protocols with minimal effort.

---

## 📦 Features

- Abstract interfaces for initializing, starting, and stopping communication sessions.
- Standardized data exchange methods for process data and service communication (e.g., ISDU for IO-Link).
- Event-based or polling mechanisms for asynchronous data reception.
- Support for multiple physical and logical ports.
- Extensibility for protocol-specific requirements (e.g., retries, diagnostic data, timing constraints).

---

## 📐 Target Use Cases

- Developers building protocol-specific communication masters (like IO-Link Master or Modbus Master).
- Systems needing a unified approach to manage different industrial device networks.
- Educational or research platforms experimenting with layered driver architectures.

---


