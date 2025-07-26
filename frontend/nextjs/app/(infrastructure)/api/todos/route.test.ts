describe('PUT', () => {
    const mockId = 'test-id';
    const mockTitle = 'Test Title';
    const mockTodo = { id: mockId, title: mockTitle };

    let mockReq: any;

    beforeEach(() => {
        jest.clearAllMocks();
        mockReq = {
            json: jest.fn().mockResolvedValue({ id: mockId, title: mockTitle }),
        };
    });

    it('should update a todo and return the updated todo', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
        const jsonSpy = jest.spyOn(require('next/server').NextResponse, 'json');
        const logSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(mockReq.json).toHaveBeenCalled();
        expect(axios.put).toHaveBeenCalledWith(
            `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
            { title: mockTitle }
        );
        expect(jsonSpy).toHaveBeenCalledWith(mockTodo);
        expect(logSpy).toHaveBeenCalledWith('title:', mockTitle);

        logSpy.mockRestore();
        jsonSpy.mockRestore();
    });

    it('should return error response if axios.put throws', async () => {
        // Arrange
        (axios.put as jest.Mock).mockRejectedValue(new Error('API error'));
        const jsonSpy = jest.spyOn(require('next/server').NextResponse, 'json');

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(jsonSpy).toHaveBeenCalledWith(
            { error: 'Failed to update todo' },
            { status: 500 }
        );

        jsonSpy.mockRestore();
    });

    it('should call axios.put with correct URL and payload', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(axios.put).toHaveBeenCalledWith(
            `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
            { title: mockTitle }
        );
    });

    it('should log the title', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
        const logSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(logSpy).toHaveBeenCalledWith('title:', mockTitle);

        logSpy.mockRestore();
    });
});
describe('PUT', () => {
    const mockId = 'mock-id';
    const mockTitle = 'mock-title';
    const mockTodo = { id: mockId, title: mockTitle };

    let mockReq: any;

    beforeEach(() => {
        jest.clearAllMocks();
        mockReq = {
            json: jest.fn().mockResolvedValue({ id: mockId, title: mockTitle }),
        };
    });

    it('returns updated todo on success', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
        const jsonSpy = jest.spyOn(require('next/server').NextResponse, 'json');
        const logSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(mockReq.json).toHaveBeenCalled();
        expect(axios.put).toHaveBeenCalledWith(
            `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
            { title: mockTitle }
        );
        expect(jsonSpy).toHaveBeenCalledWith(mockTodo);
        expect(logSpy).toHaveBeenCalledWith('title:', mockTitle);

        logSpy.mockRestore();
        jsonSpy.mockRestore();
    });

    it('returns error response if axios.put throws', async () => {
        // Arrange
        (axios.put as jest.Mock).mockRejectedValue(new Error('fail'));
        const jsonSpy = jest.spyOn(require('next/server').NextResponse, 'json');

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(jsonSpy).toHaveBeenCalledWith(
            { error: 'Failed to update todo' },
            { status: 500 }
        );

        jsonSpy.mockRestore();
    });

    it('calls axios.put with correct URL and payload', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(axios.put).toHaveBeenCalledWith(
            `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
            { title: mockTitle }
        );
    });

    it('logs the title', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
        const logSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(logSpy).toHaveBeenCalledWith('title:', mockTitle);

        logSpy.mockRestore();
    });
});
describe('PUT', () => {
    const mockId = 'unit-test-id';
    const mockTitle = 'unit-test-title';
    const mockTodo = { id: mockId, title: mockTitle };

    let mockReq: any;

    beforeEach(() => {
        jest.clearAllMocks();
        mockReq = {
            json: jest.fn().mockResolvedValue({ id: mockId, title: mockTitle }),
        };
    });

    it('returns updated todo on success', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
        const jsonSpy = jest.spyOn(require('next/server').NextResponse, 'json');
        const logSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(mockReq.json).toHaveBeenCalled();
        expect(axios.put).toHaveBeenCalledWith(
            `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
            { title: mockTitle }
        );
        expect(jsonSpy).toHaveBeenCalledWith(mockTodo);
        expect(logSpy).toHaveBeenCalledWith('title:', mockTitle);

        logSpy.mockRestore();
        jsonSpy.mockRestore();
    });

    it('returns error response if axios.put throws', async () => {
        // Arrange
        (axios.put as jest.Mock).mockRejectedValue(new Error('fail'));
        const jsonSpy = jest.spyOn(require('next/server').NextResponse, 'json');

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(jsonSpy).toHaveBeenCalledWith(
            { error: 'Failed to update todo' },
            { status: 500 }
        );

        jsonSpy.mockRestore();
    });

    it('calls axios.put with correct URL and payload', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(axios.put).toHaveBeenCalledWith(
            `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
            { title: mockTitle }
        );
    });

    it('logs the title', async () => {
        // Arrange
        (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
        const logSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

        // Act
        await require('./route').PUT(mockReq);

        // Assert
        expect(logSpy).toHaveBeenCalledWith('title:', mockTitle);

        logSpy.mockRestore();
    });
});